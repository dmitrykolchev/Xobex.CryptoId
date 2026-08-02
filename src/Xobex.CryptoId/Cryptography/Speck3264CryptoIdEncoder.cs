// <copyright file="Speck3264CryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xobex.Cryptography.Abstractions;
using Xobex.Cryptography.Algo;

namespace Xobex.Cryptography;

/// <summary>
/// Provides Speck32/64 lightweight block cipher encryption for encoding and decoding 32-bit (int) identifiers.
/// </summary>
/// <remarks>
/// <para>
/// Speck is a family of lightweight block ciphers designed by the NSA, optimized for high performance
/// in resource-constrained environments. This implementation uses Speck with 32-bit words and 64-bit key (Speck32/64).
/// </para>
/// <para>
/// Specifications:
/// - <strong>Block size:</strong> 32 bits (4 bytes input → 4 bytes output)
/// - <strong>Key size:</strong> 64 bits (8 bytes)
/// - <strong>Word size:</strong> 16 bits (n=16)
/// - <strong>Number of rounds:</strong> 22
/// - <strong>Base64Url output:</strong> 6 characters (ceil(4*8/6) = 6, without padding)
/// </para>
/// <para>
/// Reference: <see href="https://eprint.iacr.org/2013/404.pdf">NSA Speck and Simon Specification</see>, Table 2 / Algorithm 3
/// </para>
/// <para>
/// Security: This implementation is suitable for obfuscating identifiers in URLs and APIs.
/// Like all ECB-mode encryption, it does not hide patterns and should not be used for encrypting
/// sensitive data structures. For each unique plaintext, the ciphertext will always be identical
/// (deterministic encryption).
/// </para>
/// </remarks>
public sealed class Speck3264CryptoIdEncoder : ICryptoIdEncoder<int>, ICryptoIdEncoder
{
    private static readonly byte[] HkdfInfo = "Speck32-64 ID encryption v1"u8.ToArray();

    private readonly Speck32_64 _cipher;

    /// <summary>
    /// Initializes a new instance of the <see cref="Speck3264CryptoIdEncoder"/> class.
    /// </summary>
    /// <param name="key">
    /// The cryptographic key material (e.g., password, API key, or random string).
    /// Will be processed through HKDF-SHA256 to derive an 8-byte (64-bit) key for Speck32/64.
    /// </param>
    /// <param name="salt">
    /// Salt value for HKDF key derivation. Should be a cryptographically random value
    /// unique to your deployment. Typical length is 16 bytes.
    /// </param>
    /// <remarks>
    /// Decode does not perform an integrity check—the security boundary lies entirely in the
    /// authorization check after decoding, not in the successful decoding itself.
    /// For a 32-bit domain, a rate-limitless decode oracle makes exhaustive search of
    /// the space feasible locally (hours, not years) on a single machine.
    /// To create secret keys, use a cryptographically secure random sequences
    /// (e.g., RandomNumberGenerator.GetBytes(16)) and store them securely.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="salt"/> is null.</exception>
    public Speck3264CryptoIdEncoder(string key, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes for HKDF-SHA256.", nameof(salt));
        }

        // HKDF-SHA256: 8 bytes for Speck32/64 (4 words × 2 bytes)
        var keyMaterial = HKDF.DeriveKey(
                hashAlgorithmName: HashAlgorithmName.SHA256,
                ikm: Encoding.UTF8.GetBytes(key),
                outputLength: 8,
                salt: salt,
                info: HkdfInfo);

        _cipher = new Speck32_64(keyMaterial);
    }

    /// <summary>
    /// Encrypts int and encodes result to Base64Url (6 characters).
    /// </summary>
    /// <param name="id">The 32-bit integer to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    public string Encode(int id)
    {
        Span<byte> plaintext = stackalloc byte[sizeof(int)];
        Span<byte> ciphertext = stackalloc byte[sizeof(int)];

        BinaryPrimitives.WriteInt32LittleEndian(plaintext, id);
        _cipher.Encrypt(plaintext, ciphertext);

        return Base64Url.EncodeToString(ciphertext);
    }

    /// <summary>
    /// Attempts to encrypt an integer and encode it to a URL-safe Base64 string,
    /// writing the result to the provided destination span.
    /// </summary>
    /// <param name="id">The 32-bit integer to encrypt.</param>
    /// <param name="destination">The character span to write the result to.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>true if the encoding was successful; otherwise, false.</returns>
    public bool TryEncode(int id, Span<char> destination, out int charsWritten)
    {
        Span<byte> plaintext = stackalloc byte[sizeof(int)];
        Span<byte> ciphertext = stackalloc byte[sizeof(int)];

        BinaryPrimitives.WriteInt32LittleEndian(plaintext, id);
        _cipher.Encrypt(plaintext, ciphertext);

        return Base64Url.TryEncodeToChars(ciphertext, destination, out charsWritten);
    }

    /// <summary>
    /// Decodes Base64Url and decrypts to int.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <remarks>
    /// Decode does not perform an integrity check—the security boundary lies entirely in the
    /// authorization check after decoding, not in the successful decoding itself.
    /// For a 32-bit domain, a rate-limitless decode oracle makes exhaustive search of
    /// the space feasible locally (hours, not years) on a single machine.
    /// </remarks>
    /// <returns>The decrypted 32-bit integer.</returns>
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
        Span<byte> plaintext = stackalloc byte[sizeof(int)];

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

        try
        {
            _cipher.Decrypt(ciphertext, plaintext);
        }
        catch (ArgumentException ex)
        {
            return OperationResult.Fail(OperationResultKind.Failed, ex.Message);
        }

        value = BinaryPrimitives.ReadInt32LittleEndian(plaintext);
        return OperationResult.Success;
    }

    /// <inheritdoc/>
    public bool IsDeterministic => true;

    /// <inheritdoc/>
    public Type IdType => typeof(int);

    /// <inheritdoc/>
    public int IdSizeInBytes => sizeof(int);

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((int)id);
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((int)id, destination, out charsWritten);
    }

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    bool ICryptoIdEncoder.TryDecode(ReadOnlySpan<char> urlEncodedBase64, out object? value)
    {
        if(TryDecode(urlEncodedBase64, out var intValue))
        {
            value = intValue;
            return true;
        }
        value = null;
        return false;
    }
}

