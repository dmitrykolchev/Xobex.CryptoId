// <copyright file="Speck64128CryptoIdEncoder.cs" company="Dmitry Kolchev">
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
/// Provides Speck64/128 lightweight block cipher encryption for encoding and decoding 64-bit (long) identifiers.
/// </summary>
/// <remarks>
/// <para>
/// Speck is a family of lightweight block ciphers designed by the NSA, optimized for high performance
/// in resource-constrained environments. This implementation uses Speck with 32-bit words and 128-bit key (Speck64/128).
/// </para>
/// <para>
/// Specifications:
/// - <strong>Block size:</strong> 64 bits (8 bytes input → 8 bytes output)
/// - <strong>Key size:</strong> 128 bits (16 bytes)
/// - <strong>Word size:</strong> 32 bits (n=32)
/// - <strong>Number of rounds:</strong> 27
/// - <strong>Base64Url output:</strong> 11-12 characters (ceil(8*8/6) = 11)
/// </para>
/// <para>
/// Security Properties:
/// - <strong>Encryption Mode:</strong> ECB (deterministic) - intentional for ID obfuscation
/// - <strong>Key Material:</strong> HKDF-SHA256 key derivation instead of MD5
/// - <strong>Authentication:</strong> None - use with authenticated channels or add HMAC if needed
/// - <strong>Thread-safety:</strong> Guaranteed immutable after constructor
/// </para>
/// <para>
/// Reference: <see href="https://eprint.iacr.org/2013/404.pdf">NSA Speck and Simon Specification</see>, Algorithm 3
/// </para>
/// <para>
/// Note: This implementation uses deterministic ECB encryption by design. The same plaintext
/// always produces the same ciphertext. It is suitable for obfuscating identifiers in URLs and APIs
/// but should NOT be used for encrypting variable data structures or sensitive information.
/// </para>
/// </remarks>
public sealed class Speck64128CryptoIdEncoder : ICryptoIdEncoder<long>, ICryptoIdEncoder
{
    // Contextual label for HKDF — isolates key material from other applications
    private static readonly byte[] HkdfInfo = "Speck64-128 ID encryption v1"u8.ToArray();

    private readonly Speck64_128 _cipher;

    /// <summary>
    /// Initializes a new instance of the <see cref="Speck64128CryptoIdEncoder"/> class.
    /// </summary>
    /// <param name="key">
    /// The cryptographic key material (e.g., password, API key, or random string).
    /// Will be processed through HKDF-SHA256 to derive a 128-bit key for Speck64/128.
    /// </param>
    /// <param name="salt">
    /// Salt value for HKDF key derivation. Should be a cryptographically random value
    /// unique to your deployment. Typical length is 16 bytes.
    /// In production, provide a unique salt per deployment explicitly.
    /// </param>
    /// <remarks>
    /// Decode does not perform an integrity check—the security boundary lies entirely in the
    /// authorization check after decoding, not in the successful decoding itself.
    /// Exhaustive search of the 64-bit domain is not feasible, but keep the key and salt stable
    /// so that previously issued IDs remain decodable.
    /// To create secret keys, use a cryptographically secure random sequence
    /// (e.g., RandomNumberGenerator.GetBytes(16)) and store it securely.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="salt"/> is null.</exception>
    public Speck64128CryptoIdEncoder(string key, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes for HKDF-SHA256.", nameof(salt));
        }

        // HKDF-SHA256: ikm → 16-bytes key for Speck64/128
        // Unlike MD5: cryptographically secure, domain-separated, without collision vulnerabilities
        var keyMaterial = HKDF.DeriveKey(
            hashAlgorithmName: HashAlgorithmName.SHA256,
            ikm: Encoding.UTF8.GetBytes(key),
            outputLength: 16,
            salt: salt,
            info: HkdfInfo);

        _cipher = new Speck64_128(keyMaterial);
    }

    /// <summary>
    /// Encrypts a 64-bit (long) identifier and encodes it to a URL-safe Base64 string.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string (approximately 11-12 characters).</returns>
    public string Encode(long id)
    {
        Span<byte> plaintext = stackalloc byte[sizeof(long)];
        Span<byte> ciphertext = stackalloc byte[sizeof(long)];

        // Explicit little-endian - deterministic behavior on any platform
        BinaryPrimitives.WriteInt64LittleEndian(plaintext, id);
        _cipher.Encrypt(plaintext, ciphertext);

        return Base64Url.EncodeToString(ciphertext);
    }

    /// <summary>
    /// Attempts to encrypt a 64-bit (long) identifier and encode it to a URL-safe Base64 string,
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="urlEncodedBase64">The span to write the encoded string to.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>true if the encoding was successful; otherwise, false.</returns>
    public bool TryEncode(long id, Span<char> urlEncodedBase64, out int charsWritten)
    {
        Span<byte> plaintext = stackalloc byte[sizeof(long)];
        Span<byte> ciphertext = stackalloc byte[sizeof(long)];

        BinaryPrimitives.WriteInt64LittleEndian(plaintext, id);
        _cipher.Encrypt(plaintext, ciphertext);

        return Base64Url.TryEncodeToChars(ciphertext, urlEncodedBase64, out charsWritten);
    }

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to a 64-bit (long) identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <remarks>
    /// Decode does not perform an integrity check—the security boundary lies entirely in the
    /// authorization check after decoding, not in the successful decoding itself.
    /// </remarks>
    /// <returns>The decrypted identifier.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid URL-safe Base64 string or contains invalid data.</exception>
    public long Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        Span<byte> ciphertext = stackalloc byte[sizeof(long)];
        Span<byte> plaintext = stackalloc byte[sizeof(long)];

        if (!Base64Url.TryDecodeFromChars(urlEncodedBase64, ciphertext, out var bytesWritten)
            || bytesWritten != sizeof(long))
        {
            throw new FormatException(
                $"Invalid Base64Url format: expected {sizeof(long)} bytes after decoding.");
        }

        _cipher.Decrypt(ciphertext, plaintext);

        return BinaryPrimitives.ReadInt64LittleEndian(plaintext);
    }

    /// <inheritdoc/>
    public bool TryDecode(ReadOnlySpan<char> urlEncodedBase64, out long value)
    {
        Span<byte> ciphertext = stackalloc byte[sizeof(long)];
        Span<byte> plaintext = stackalloc byte[sizeof(long)];

        value = default;
        try
        {
            if (!Base64Url.TryDecodeFromChars(urlEncodedBase64, ciphertext, out var bytesWritten)
                || bytesWritten != sizeof(long))
            {
                return false;
            }
        }
        catch (FormatException)
        {
            return false;
        }

        try
        {
            _cipher.Decrypt(ciphertext, plaintext);
        }
        catch (ArgumentException)
        {
            return false;
        }
        value = BinaryPrimitives.ReadInt64LittleEndian(plaintext);
        return true;
    }

    /// <inheritdoc/>
    public bool IsDeterministic => true;

    /// <inheritdoc/>
    public Type IdType => typeof(long);

    /// <inheritdoc/>
    public int IdSizeInBytes => sizeof(long);

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((long)id);
    }

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    bool ICryptoIdEncoder.TryDecode(ReadOnlySpan<char> urlEncodedBase64, out object? value)
    {
        if (TryDecode(urlEncodedBase64, out var longValue))
        {
            value = longValue;
            return true;
        }
        value = null;
        return false;
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((long)id, destination, out charsWritten);
    }

}

