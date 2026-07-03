// <copyright file="CompactDeterministicAesCryptoIdEncoder.cs" company="Dmitry Kolchev">
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
/// Provides ultra-compact deterministic encryption of 64-bit (long) identifiers
/// using a single AES-256 block with built-in integrity checking.
/// </summary>
/// <remarks>
/// <para>
/// This method encodes an 8-byte ID and an 8-byte verification cipher (truncated HMAC-SHA256)
/// into a single 16-byte AES block. This results in a fixed encrypted data size (16 bytes),
/// which, when encoded in Base64Url, yields a string of only 22 characters (instead of 48 characters for AES-GCM).
/// </para>
/// <para>
/// Security Properties:
/// - **Confidentiality**: AES-256 in ECB mode. Since the data length is strictly equal to the size of one
/// block (16 bytes), ECB mode is secure because it is a pseudo-random permutation (PRP).
/// - **Integrity**: Use of a 64-bit HMAC-SHA256 key-forgery guarantees protection
/// from brute-force attacks and unauthorized modification (probability of successful forgery: 1/2⁶⁴).
/// - **Key Sharing**: Using HKDF-SHA256, two independent keys are generated:
/// one for AES encryption, the other for HMAC calculation.
/// </para>
/// </remarks>
public sealed class CompactDeterministicAesCryptoIdEncoder : IDisposable, ICryptoIdEncoder<long>, ICryptoIdEncoder
{
    private const int BlockSize = 16;
    private const int IdSize = sizeof(long);

    private static readonly byte[] HkdfAesInfo = "Xobex.AesEcbCompact.Encrypt.v1"u8.ToArray();
    private static readonly byte[] HkdfMacInfo = "Xobex.AesEcbCompact.Mac.v1"u8.ToArray();

    private readonly ThreadLocal<Aes> _cipher;
    private readonly byte[] _macKey;
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
        var aesKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 32, salt, HkdfAesInfo);
        _macKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 32, salt, HkdfMacInfo);

        // Инициализируем пул AES для каждого потока
        _cipher = new(() =>
        {
            var aes = Aes.Create();
            aes.Key = aesKey;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            return aes;
        }, trackAllValues: true);
    }

    private Aes Cipher
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _cipher.Value!;
        }
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

        // Encrypting a block in place (ECB mode without padding for one block)
        Cipher.EncryptEcb(block, encryptedBlock, PaddingMode.None);
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
        Cipher.DecryptEcb(encryptedBlock, decryptedBlock, PaddingMode.None);

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

        foreach (var aes in _cipher.Values)
        {
            aes?.Dispose();
        }
        _cipher.Dispose();
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
}
