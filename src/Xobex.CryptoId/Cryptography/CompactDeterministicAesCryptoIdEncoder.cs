// <copyright file="CompactDeterministicAesCryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xobex.Cryptography.Abstractions;

namespace Xobex.Cryptography;

/// <summary>
/// Provides a compact, deterministic encryption scheme for 64-bit identifiers using AES-256.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is specifically designed for scenarios where URL length is a 
/// critical constraint. It achieves a highly compact 22-character Base64Url output 
/// by encrypting a single 16-byte block containing the ID and a 64-bit FNV-1a checksum.
/// </para>
/// <para>
/// <b>Security Warning:</b> This is a <b>Deterministic Encryption</b> scheme. 
/// Unlike AES-GCM, it does not use a unique nonce per encryption, making it 
/// susceptible to pattern analysis and replay attacks. The integrity check is 
/// provided by a non-cryptographic checksum (FNV-1a) appended to the plaintext 
/// before encryption.
/// </para>
/// <para>
/// <b>Use Case:</b> Suitable for obfuscating database primary keys in URLs to 
/// prevent simple sequential enumeration (IDOR protection), provided that 
/// authorization is enforced on the server side.
/// </para>
/// </remarks>
public sealed class CompactDeterministicAesCryptoIdEncoder : IDisposable, ICryptoIdEncoder<long>, ICryptoIdEncoder
{
    private const int BlockSize = 16;
    private const int IdSize = sizeof(long);

    private static readonly byte[] HkdfAesInfo = "Xobex.AesEcbCompact.Encrypt.v1"u8.ToArray();

    private readonly DisposableObjectPool<Aes> _pool;
    private readonly byte[] _keyMaterial;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactDeterministicAesCryptoIdEncoder"/> class.
    /// </summary>
    /// <param name="key">The encryption key.</param>
    /// <param name="salt">The salt for HKDF.</param>
    public CompactDeterministicAesCryptoIdEncoder(string key, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes for HKDF-SHA256.", nameof(salt));
        }

        var ikm = Encoding.UTF8.GetBytes(key);
        // Криптографическое разделение ключей (Key Separation)
        _keyMaterial = HKDF.DeriveKey(
            hashAlgorithmName: HashAlgorithmName.SHA256,
            ikm: ikm,
            outputLength: 32,
            salt: salt,
            info: HkdfAesInfo);
        CryptographicOperations.ZeroMemory(ikm);
        _pool = new DisposableObjectPool<Aes>(CreatePooledInstance);
    }

    private Aes CreatePooledInstance()
    {
        var newAes = Aes.Create();
        newAes.Key = _keyMaterial;
        newAes.Mode = CipherMode.ECB;
        newAes.Padding = PaddingMode.None;
        return newAes;
    }

    /// <summary>
    /// Encodes a 64-bit ID into a URL-safe Base64 string of 22 characters.
    /// </summary>
    /// <param name="id">The ID to encode.</param>
    /// <returns>The encoded ID as a URL-safe Base64 string.</returns>
    public string Encode(long id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> encryptedBlock = stackalloc byte[BlockSize];

        EncryptInternal(encryptedBlock, id);

        return Base64Url.EncodeToString(encryptedBlock);
    }

    /// <summary>
    /// Attempts to encode a 64-bit ID into a URL-safe Base64 string, writing the result to the provided character span.
    /// </summary>
    /// <param name="id">The ID to encode.</param>
    /// <param name="destination">The span to write the encoded string to.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>true if the encoding was successful; otherwise, false.</returns>
    public bool TryEncode(long id, Span<char> destination, out int charsWritten)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> encryptedBlock = stackalloc byte[BlockSize];

        EncryptInternal(encryptedBlock, id);

        return Base64Url.TryEncodeToChars(encryptedBlock, destination, out charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EncryptInternal(Span<byte> encryptedBlock, long id)
    {
        Span<byte> block = stackalloc byte[BlockSize];
        var idSpan = block[..IdSize];
        var tagSpan = block[IdSize..];

        BinaryPrimitives.WriteInt64LittleEndian(idSpan, id);

        // Calculate HMAC from ID to check integrity (without intermediate copies)
        // 2. Вычисляем сверхбыстрый FNV-1a на стеке (~1-2 нс)
        var hash = ComputeFnv1a64(idSpan);
        BinaryPrimitives.WriteUInt64LittleEndian(tagSpan, hash);

        using var cipher = _pool.LeaseObject();
        // Encrypting a block in place (ECB mode without padding for one block)
        cipher.Instance.EncryptEcb(block, encryptedBlock, PaddingMode.None);
    }

    /// <summary>
    /// Decodes a URL-safe Base64 string and verifies its integrity.
    /// </summary>
    /// <param name="text">The URL-safe Base64 string to decode.</param>
    /// <returns>The decoded ID.</returns>
    public long Decode(ReadOnlySpan<char> text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> encryptedBlock = stackalloc byte[BlockSize];

        if (!Base64Url.TryDecodeFromChars(text, encryptedBlock, out var bytesWritten) || bytesWritten != BlockSize)
        {
            throw new FormatException($"Invalid Base64Url format: expected {BlockSize} bytes after decoding.");
        }

        // Decrypting the block
        Span<byte> decryptedBlock = stackalloc byte[BlockSize];

        using (var cipher = _pool.LeaseObject())
        {
            cipher.Instance.DecryptEcb(encryptedBlock, decryptedBlock, PaddingMode.None);
        }

        var idSpan = decryptedBlock[..IdSize];
        var tagSpan = decryptedBlock[IdSize..];

        // 2. Считаем и сверяем FNV-1a
        var expectedHash = ComputeFnv1a64(idSpan);
        var actualHash = BinaryPrimitives.ReadUInt64LittleEndian(tagSpan);
        if (expectedHash != actualHash)
        {
            throw new CryptographicException("Integrity check failed: ID cipher text is modified or invalid.");
        }

        // Returning the original ID
        return BinaryPrimitives.ReadInt64LittleEndian(idSpan);
    }

    /// <inheritdoc/>
    public bool TryDecode(ReadOnlySpan<char> text, out long value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        value = default;
        Span<byte> encryptedBlock = stackalloc byte[BlockSize];

        if (!Base64Url.TryDecodeFromChars(text, encryptedBlock, out var bytesWritten) || bytesWritten != BlockSize)
        {
            return false;
        }

        // Decrypting the block
        Span<byte> decryptedBlock = stackalloc byte[BlockSize];

        try
        {
            using var cipher = _pool.LeaseObject();
            cipher.Instance.DecryptEcb(encryptedBlock, decryptedBlock, PaddingMode.None);
        }
        catch (CryptographicException)
        {
            return false;
        }
        var idSpan = decryptedBlock[..IdSize];
        var tagSpan = decryptedBlock[IdSize..];

        // 2. Считаем и сверяем FNV-1a
        var expectedHash = ComputeFnv1a64(idSpan);
        var actualHash = BinaryPrimitives.ReadUInt64LittleEndian(tagSpan);
        if (expectedHash != actualHash)
        {
            return false;
        }

        // Returning the original ID
        value = BinaryPrimitives.ReadInt64LittleEndian(idSpan);
        return true;
    }

    /// <summary>
    /// Вычисляет 64-битный некриптографический хеш FNV-1a.
    /// Алгоритм выполняется за O(N) по байтам, для 8 байт это всего 8 итераций XOR/MUL.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ComputeFnv1a64(ReadOnlySpan<byte> data)
    {
        var hash = 14695981039346656037UL; // FNV offset basis
        foreach (var b in data)
        {
            hash ^= b;
            hash *= 1099511628211UL; // FNV prime
        }
        return hash;
    }

    /// <inheritdoc/>
    public bool IsDeterministic => true;

    /// <inheritdoc/>
    public Type IdType => typeof(long);

    /// <inheritdoc/>
    public int IdSizeInBytes => sizeof(long);

    /// <summary>
    /// Releases all resources used by the <see cref="CompactDeterministicAesCryptoIdEncoder"/> instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _pool.Dispose();
        CryptographicOperations.ZeroMemory(_keyMaterial);
    }

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((long)id);
    }

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((long)id, destination, out charsWritten);
    }

    bool ICryptoIdEncoder.TryDecode(ReadOnlySpan<char> urlEncodedBase64, out object value)
    {
        var result = TryDecode(urlEncodedBase64, out var id);
        value = id;
        return result;
    }
}
