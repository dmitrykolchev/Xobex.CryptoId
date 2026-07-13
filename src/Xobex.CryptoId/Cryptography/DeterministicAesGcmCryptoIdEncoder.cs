// <copyright file="DeterministicAesGcmCryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xobex.Cryptography.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace Xobex.Cryptography;

/// <summary>
/// Provides deterministic AES-GCM encryption for encoding and decoding 64-bit (long) identifiers.
/// </summary>
/// <remarks>
/// <para>
/// This encoder uses AES-GCM (Advanced Encryption Standard in Galois/Counter Mode) to encrypt
/// 64-bit identifiers and encode them as URL-safe Base64 strings. Each encryption operation uses
/// a random nonce to ensure different ciphertexts for the same plaintext.
/// </para>
/// <para>
/// Security Properties:
/// - **Encryption**: AES-256-GCM with random 96-bit nonce and 128-bit authentication tag
/// - **Key Material**: HKDF-SHA256 key derivation from user-provided key and salt
/// - **Authentication**: Each ciphertext includes authenticated tag for integrity verification
/// - **Randomness**: New nonce for each encryption operation (cryptographically secure)
/// </para>
/// <para>
/// Nonce Collision Risk:
/// For random 96-bit nonces, collision probability with n messages is approximately n²/2⁹⁷.
/// With n = 2³² messages: P(collision) ≈ 2⁻³³, which is unacceptable for high-volume services.
/// This implementation is suitable for general-purpose ID obfuscation but not for systems
/// requiring cryptographic strength against sophisticated attackers. For higher security,
/// consider using deterministic nonce counters or nonce-misuse-resistant modes like AES-GCM-SIV.
/// </para>
/// </remarks>
public sealed class DeterministicAesGcmCryptoIdEncoder : IDisposable, ICryptoIdEncoder<long>, ICryptoIdEncoder
{
    private const int TagSize = 16;
    private const int NonceSize = 12;
    private const int CipherTextSize = sizeof(long);
    private const int TotalSize = NonceSize + TagSize + CipherTextSize;


    // Contextual label for HKDF — isolates key material from other applications
    private static readonly byte[] HkdfInfo = "AES ID encryption v1"u8.ToArray();
    private static readonly byte[] NonceKeyInfo = "Xobex.AesGcm.CryptoId.nonce.v1"u8.ToArray();

    private readonly DisposableObjectPool<AesGcm> _pool;
    private readonly byte[] _keyMaterial;
    private readonly byte[] _nonceKey;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeterministicAesGcmCryptoIdEncoder"/> class.
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
    public DeterministicAesGcmCryptoIdEncoder(string key, byte[] salt)
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
        _keyMaterial = HKDF.DeriveKey(
            hashAlgorithmName: HashAlgorithmName.SHA256,
            ikm: ikm,
            outputLength: 32,
            salt: salt,
            info: HkdfInfo);
        _pool = new DisposableObjectPool<AesGcm>(() => new AesGcm(_keyMaterial, TagSize));
        CryptographicOperations.ZeroMemory(ikm);
    }

    /// <summary>
    /// Encrypts a 64-bit (long) identifier and encodes it to a URL-safe Base64 string.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    /// <remarks>
    /// The encryption is deterministic (Synthetic IV) and the nonce is calculated based on HMAC-SHA256 from ID
    /// </remarks>
    public string Encode(long id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Span<byte> buffer = stackalloc byte[TotalSize];
        EncryptInternal(buffer, id);
        return Base64Url.EncodeToString(buffer);
    }

    /// <summary>
    /// Attempts to encrypt a 64-bit (long) identifier and encode it to a URL-safe Base64
    /// string, writing the result to the provided character span.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="destination">The character span to write the result to.</param>
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
        var idBytes = buffer.Slice(NonceSize + TagSize, CipherTextSize);
        BinaryPrimitives.WriteInt64LittleEndian(idBytes, id);

        var nonce = buffer[..NonceSize];
        ComputeNonce(idBytes, nonce);

        var tag = buffer.Slice(NonceSize, TagSize);
        using var cipher = _pool.LeaseObject();
        cipher.Instance.Encrypt(nonce, idBytes, idBytes, tag);
    }

    /// <summary>
    /// Decrypts text to long id
    /// </summary>
    /// <param name="text">encrypted id</param>
    /// <returns>decrypted id</returns>
    /// <exception cref="FormatException">Invalid Base64Url format</exception>
    public long Decode(ReadOnlySpan<char> text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> buffer = stackalloc byte[TotalSize];

        // Decode URL-Base64 back to bytes on the stack in a single pass without allocations
        if (!Base64Url.TryDecodeFromChars(text, buffer, out var bytesWritten) || bytesWritten != TotalSize)
        {
            throw new FormatException($"Invalid Base64Url format: expected {TotalSize} bytes after decoding.");
        }

        ReadOnlySpan<byte> nonce = buffer[..NonceSize];
        ReadOnlySpan<byte> tag = buffer.Slice(NonceSize, TagSize);
        ReadOnlySpan<byte> ciphertext = buffer.Slice(NonceSize + TagSize, CipherTextSize);

        Span<byte> plaintext = stackalloc byte[CipherTextSize];
        using var cipher = _pool.LeaseObject();
        cipher.Instance.Decrypt(nonce, ciphertext, tag, plaintext);
        // Read int64 with correct byte orderт
        return BinaryPrimitives.ReadInt64LittleEndian(plaintext);
    }

    /// <inheritdoc/>
    public bool TryDecode(ReadOnlySpan<char> urlEncodedBase64, out long value)
    {
        value = default;
        if(_disposed)
        {
            return false;
        }
        Span<byte> buffer = stackalloc byte[TotalSize];

        try
        {
            // Decode URL-Base64 back to bytes on the stack in a single pass without allocations
            if (!Base64Url.TryDecodeFromChars(urlEncodedBase64, buffer, out var bytesWritten) || bytesWritten != TotalSize)
            {
                throw new FormatException($"Invalid Base64Url format: expected {TotalSize} bytes after decoding.");
            }
        }
        catch (FormatException)
        {
            return false;
        }

        ReadOnlySpan<byte> nonce = buffer[..NonceSize];
        ReadOnlySpan<byte> tag = buffer.Slice(NonceSize, TagSize);
        ReadOnlySpan<byte> ciphertext = buffer.Slice(NonceSize + TagSize, CipherTextSize);

        Span<byte> plaintext = stackalloc byte[CipherTextSize];
        try
        {
            using var cipher = _pool.LeaseObject();
            cipher.Instance.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch(CryptographicException)
        {
            return false;
        }
        // Read int64 with correct byte orderт
        value = BinaryPrimitives.ReadInt64LittleEndian(plaintext);
        return true;
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
        _pool.Dispose();
        CryptographicOperations.ZeroMemory(_keyMaterial);
        CryptographicOperations.ZeroMemory(_nonceKey);
    }

    string ICryptoIdEncoder.Encode(object id)
    {
        return Encode((long)id);
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((long)id, destination, out charsWritten);
    }
    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    bool ICryptoIdEncoder.TryDecode(ReadOnlySpan<char> urlEncodedBase64, out object value)
    {
        var result = TryDecode(urlEncodedBase64, out var id);
        value = id;
        return result;
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

