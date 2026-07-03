// <copyright file="Skip32CryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Xobex.Cryptography.Abstractions;

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
    /// Will be processed through HKDF-SHA256 to derive an 8-byte (64-bit) key for Speck32/64.
    /// </param>
    /// <param name="salt">
    /// Salt value for HKDF key derivation. Should be a cryptographically random value
    /// unique to your deployment. Typical length is 16 bytes.
    /// </param>
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

    /// <summary>
    /// Decodes Base64Url and decrypts to int.
    /// </summary>
    /// <param name="urlEncodedBase64">encoded and encrypted value</param>
    /// <returns>decoded and decrypted int</returns>
    /// <exception cref="FormatException">Invalid Base64Url format.</exception>
    public int Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        Span<byte> ciphertext = stackalloc byte[sizeof(int)];

        if (!Base64Url.TryDecodeFromChars(urlEncodedBase64, ciphertext, out var bytesWritten)
            || bytesWritten != sizeof(int))
        {
            throw new FormatException(
                $"Invalid Base64Url format: expected {sizeof(int)} bytes after decoding.");
        }

        var decrypted = _cipher.Decrypt(MemoryMarshal.Cast<byte, uint>(ciphertext)[0]);

        return unchecked((int)decrypted);
    }

    /// <summary>
    /// Encrypts int and encodes result to Base64Url.
    /// </summary>
    /// <param name="id">value to encrypt</param>
    /// <returns>encrypted and encoded base64 string</returns>
    public string Encode(int id)
    {
        var encrypted = _cipher.Encrypt(unchecked((uint)id));
        Span<byte> ciphertext = stackalloc byte[sizeof(int)];
        return Base64Url.EncodeToString(MemoryMarshal.Cast<uint, byte>(MemoryMarshal.CreateReadOnlySpan<uint>(ref encrypted, 1)));
    }

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((int)id);
    }
}
