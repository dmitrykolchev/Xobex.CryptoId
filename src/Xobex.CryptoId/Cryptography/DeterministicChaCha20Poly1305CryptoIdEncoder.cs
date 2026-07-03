// <copyright file="DeterministicChaCha20Poly1305CryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xobex.Cryptography.Abstractions;

namespace Xobex.Cryptography;

/// <summary>
/// Provides deterministic ChaCha20-Poly1305 authenticated encryption for encoding and decoding 64-bit (long) identifiers.
/// </summary>
public sealed class DeterministicChaCha20Poly1305CryptoIdEncoder : ICryptoIdEncoder<long>, ICryptoIdEncoder, IDisposable
{
    private const int TagSize = 16;
    private const int NonceSize = 12;
    private const int IdSize = sizeof(long);
    private const int TotalSize = NonceSize + TagSize + IdSize;

    // Contextual label for HKDF — isolates key material from other applications
    private static readonly byte[] HkdfInfo = "ChaCha20Poly1305 ID encryption v1"u8.ToArray();
    private static readonly byte[] NonceKeyInfo = "Xobex.ChaCha20Poly1305.CryptoId.nonce.v1"u8.ToArray();

    private readonly ChaCha20Poly1305 _cipher;
    private readonly byte[] _nonceKey;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeterministicChaCha20Poly1305CryptoIdEncoder"/> class.
    /// </summary>
    /// <param name="key">
    /// The cryptographic key material (e.g., password, API key, or random string).
    /// Will be processed through HKDF-SHA256 to derive a 256-bit key for AES-256.
    /// </param>
    /// <param name="salt">
    /// Salt value for HKDF key derivation. Should be a cryptographically random value
    /// unique to your deployment. Typical length is 16 bytes.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="salt"/> is null.</exception>
    public DeterministicChaCha20Poly1305CryptoIdEncoder(string key, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes for HKDF-SHA256.", nameof(salt));
        }

        var ikm = Encoding.UTF8.GetBytes(key);
        _nonceKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 32, salt, NonceKeyInfo);

        // HKDF-SHA256: ikm → 32-байтный ключ для AES
        var keyMaterial = HKDF.DeriveKey(
            hashAlgorithmName: HashAlgorithmName.SHA256,
            ikm: ikm,
            outputLength: 32,
            salt: salt,
            info: HkdfInfo);
        _cipher = new ChaCha20Poly1305(keyMaterial);
    }

    /// <summary>
    /// Gets the thread-local ChaCha20Poly1305 cipher instance.
    /// </summary>
    /// <value>The ChaCha20Poly1305 cipher instance for this thread.</value>
    /// <exception cref="ObjectDisposedException">Thrown if the encoder has been disposed.</exception>
    private ChaCha20Poly1305 Cipher
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _cipher;
        }
    }

    /// <summary>
    /// Encodes a 64-bit identifier into a URL-safe Base64 string using ChaCha20-Poly1305 encryption.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    public string Encode(long id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> buffer = stackalloc byte[TotalSize];

        EncryptInternal(buffer, id);

        return Base64Url.EncodeToString(buffer);
    }

    /// <summary>
    /// Tries to encode a 64-bit identifier into a URL-safe Base64 string using ChaCha20-Poly1305 encryption.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="destination">The span to write the encoded string to.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>true if the encoding was successful; otherwise, false.</returns>
    public bool TryEncode(long id, Span<char> destination, out int charsWritten)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> buffer = stackalloc byte[TotalSize];

        EncryptInternal(buffer, id);

        return Base64Url.TryEncodeToChars(buffer, destination, out charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EncryptInternal(Span<byte> buffer, long id)
    {
        var idBytes = buffer.Slice(NonceSize + TagSize, IdSize);
        BinaryPrimitives.WriteInt64LittleEndian(idBytes, id);

        var nonce = buffer[..NonceSize];
        ComputeNonce(idBytes, nonce);

        var tag = buffer.Slice(NonceSize, TagSize);

        Cipher.Encrypt(nonce, idBytes, idBytes, tag);
    }

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original 64-bit identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">encrypted id</param>
    /// <returns>The original 64-bit identifier.</returns>
    /// <exception cref="FormatException">Thrown if the input is not a valid URL-safe Base64 string.</exception>
    public long Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        const int ciphertextSize = sizeof(long);
        const int totalSize = NonceSize + TagSize + ciphertextSize;

        Span<byte> buffer = stackalloc byte[totalSize];

        // Decode URL-Base64 back to bytes on the stack in a single pass without allocations
        if (!Base64Url.TryDecodeFromChars(urlEncodedBase64, buffer, out var bytesWritten) || bytesWritten != totalSize)
        {
            throw new FormatException($"Invalid Base64Url format: expected {totalSize} bytes after decoding.");
        }

        ReadOnlySpan<byte> nonce = buffer[..NonceSize];
        ReadOnlySpan<byte> tag = buffer.Slice(NonceSize, TagSize);
        ReadOnlySpan<byte> ciphertext = buffer.Slice(NonceSize + TagSize, ciphertextSize);

        Span<byte> plaintext = stackalloc byte[ciphertextSize];
        Cipher.Decrypt(nonce, ciphertext, tag, plaintext);
        // Read int64 with correct byte orderт
        return BinaryPrimitives.ReadInt64LittleEndian(plaintext);
    }

    /// <inheritdoc/>
    public bool IsDeterministic => true;

    /// <inheritdoc/>
    public Type IdType => typeof(long);

    /// <inheritdoc/>
    public int IdSizeInBytes => sizeof(long);

    /// <summary>
    /// Disposes the encoder and releases all associated resources, including the thread-local cipher.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _cipher.Dispose();
    }

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((long)id);
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((long)id, destination, out charsWritten);
    }

    /// <summary>
    /// Computes a deterministic 12-byte nonce as the truncated HMAC-SHA256 over the format
    /// version and the plaintext id, keyed with a key that is independent of the encryption key.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeNonce(ReadOnlySpan<byte> idBytes, Span<byte> nonceDestination)
    {
        Span<byte> mac = stackalloc byte[32];
        HMACSHA256.HashData(_nonceKey, idBytes, mac);
        mac[..NonceSize].CopyTo(nonceDestination);
    }
}
