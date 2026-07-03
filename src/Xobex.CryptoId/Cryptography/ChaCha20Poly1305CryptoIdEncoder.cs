// <copyright file="ChaCha20Poly1305CryptoIdEncoder.cs" company="Dmitry Kolchev">
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
/// Provides ChaCha20-Poly1305 authenticated encryption for encoding and decoding 64-bit (long) identifiers.
/// </summary>
public sealed class ChaCha20Poly1305CryptoIdEncoder : ICryptoIdEncoder<long>, ICryptoIdEncoder, IDisposable
{
    private const int TagSize = 16;
    private const int NonceSize = 12;

    // Contextual label for HKDF — isolates key material from other applications
    private static readonly byte[] HkdfInfo = "ChaCha20Poly1305 ID encryption v1"u8.ToArray();

    private readonly ChaCha20Poly1305 _cipher;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChaCha20Poly1305CryptoIdEncoder"/> class.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="salt"></param>
    public ChaCha20Poly1305CryptoIdEncoder(string key, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes for HKDF-SHA256.", nameof(salt));
        }

        // HKDF-SHA256: ikm → 32-байтный ключ для AES
        var keyMaterial = HKDF.DeriveKey(
            hashAlgorithmName: HashAlgorithmName.SHA256,
            ikm: Encoding.UTF8.GetBytes(key),
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
    /// Decodes a URL-safe Base64 string and decrypts it back to the original 64-bit identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">encrypted id</param>
    /// <returns>The original 64-bit identifier.</returns>
    /// <exception cref="FormatException">Thrown if the input is not a valid URL-safe Base64 string.</exception>
    public long Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
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

    /// <summary>
    /// Encodes a 64-bit identifier into a URL-safe Base64 string using ChaCha20-Poly1305 encryption.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    public string Encode(long id)
    {
        const int ciphertextSize = sizeof(long);
        const int totalSize = NonceSize + TagSize + ciphertextSize;
        // Everything is allocated on the stack - zero heap allocations

        Span<byte> buffer = stackalloc byte[totalSize];
        var nonce = buffer[..NonceSize];
        var tag = buffer.Slice(NonceSize, TagSize);
        var ciphertext = buffer.Slice(NonceSize + TagSize, ciphertextSize);

        RandomNumberGenerator.Fill(nonce);
        BinaryPrimitives.WriteInt64LittleEndian(ciphertext, id);

        Cipher.Encrypt(nonce, ciphertext, ciphertext, tag);

        // .NET magic: Encodes directly to URL-safe string in a single pass.
        // This creates exactly one string allocation
        return Base64Url.EncodeToString(buffer);
    }

    /// <inheritdoc/>
    public bool IsDeterministic => false;


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
}
