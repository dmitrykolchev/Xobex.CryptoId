// <copyright file="Skip32CryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Xobex.Cryptography.Abstractions;
using Xobex.Cryptography.Algo;

namespace Xobex.Cryptography;

/// <summary>
/// Provides Skip32 lightweight block cipher encryption for encoding and decoding 32-bit (int) identifiers.
/// </summary>
public sealed class Skip32CryptoIdEncoder : ICryptoIdEncoder<int>, ICryptoIdEncoder
{
    // Contextual label for HKDF — isolates key material from other applications
    private static readonly byte[] HkdfInfo = "Skip32 ID encryption v1"u8.ToArray();
    private readonly Skip32 _cipher;

    /// <summary>
    /// Initializes a new instance of the <see cref="Skip32CryptoIdEncoder"/> class.
    /// </summary>
    /// <param name="key">
    /// The cryptographic key material (e.g., password, API key, or random string).
    /// Will be processed through HKDF-SHA256 to derive a 10-byte (80-bit) key for Skip32.
    /// </param>
    /// <param name="salt">
    /// Salt value for HKDF key derivation. Should be a cryptographically random value
    /// unique to your deployment. Typical length is 16 bytes.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="salt"/> is null.</exception>
    public Skip32CryptoIdEncoder(string key, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes for HKDF-SHA256.", nameof(salt));
        }

        var keyMaterial = HKDF.DeriveKey(
            hashAlgorithmName: HashAlgorithmName.SHA256,
            ikm: Encoding.UTF8.GetBytes(key),
            outputLength: 10,
            salt: salt,
            info: HkdfInfo);

        _cipher = new Skip32(keyMaterial);
    }

    /// <inheritdoc/>
    public bool IsDeterministic => true;

    /// <inheritdoc/>
    public Type IdType => typeof(int);

    /// <inheritdoc/>
    public int IdSizeInBytes => sizeof(int);

    /// <summary>
    /// Decodes Base64Url and decrypts to int.
    /// </summary>
    /// <param name="urlEncodedBase64">encoded and encrypted value</param>
    /// <returns>decoded and decrypted int</returns>
    /// <exception cref="FormatException">Invalid Base64Url format.</exception>
    public int Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        TryDecodeInternal(urlEncodedBase64, out var value).ThrowIfFailed();
        return value;
    }

    /// <inheritdoc/>
    public bool TryDecode(ReadOnlySpan<char> urlEncodedBase64, out int value)
    {
        return TryDecodeInternal(urlEncodedBase64, out value).Succeeded;
    }

    private OperationResult TryDecodeInternal(ReadOnlySpan<char> text, out int value)
    {
        value = default;

        Span<byte> ciphertext = stackalloc byte[sizeof(int)];

        try
        {
            if (!Base64Url.TryDecodeFromChars(text, ciphertext, out var bytesWritten)
                || bytesWritten != sizeof(int))
            {
                return OperationResult.Fail(OperationResultKind.FormatError, $"Invalid Base64Url format: expected {sizeof(int)} bytes after decoding.");
            }
        }
        catch (FormatException ex)
        {
            return OperationResult.Fail(OperationResultKind.FormatError, ex.Message);
        }

        var decrypted = _cipher.Decrypt(BinaryPrimitives.ReadUInt32LittleEndian(ciphertext));
        value = unchecked((int)decrypted);
        return OperationResult.Success;
    }

    /// <summary>
    /// Encrypts int and encodes result to Base64Url.
    /// </summary>
    /// <param name="id">value to encrypt</param>
    /// <returns>encrypted and encoded base64 string</returns>
    public string Encode(int id)
    {
        Span<byte> ciphertext = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(ciphertext, _cipher.Encrypt(unchecked((uint)id)));
        return Base64Url.EncodeToString(ciphertext);
    }

    /// <summary>
    /// Attempts to encrypt an int and encode it to a URL-safe Base64 string, writing the result to the provided character span.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="destination">The character span to write the result to.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>true if the encoding was successful; otherwise, false.</returns>
    public bool TryEncode(int id, Span<char> destination, out int charsWritten)
    {
        Span<byte> ciphertext = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(ciphertext, _cipher.Encrypt(unchecked((uint)id)));
        return Base64Url.TryEncodeToChars(ciphertext, destination, out charsWritten);
    }

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    bool ICryptoIdEncoder.TryDecode(ReadOnlySpan<char> urlEncodedBase64, out object? value)
    {
        if (TryDecode(urlEncodedBase64, out var intValue))
        {
            value = intValue;
            return true;
        }
        value = null;
        return false;
    }

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((int)id);
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((int)id, destination, out charsWritten);
    }
}
