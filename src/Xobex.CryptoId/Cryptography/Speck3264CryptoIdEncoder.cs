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
        Span<byte> ciphertext = stackalloc byte[sizeof(int)];
        Span<byte> plaintext = stackalloc byte[sizeof(int)];

        if (!Base64Url.TryDecodeFromChars(urlEncodedBase64, ciphertext, out var bytesWritten)
            || bytesWritten != sizeof(int))
        {
            throw new FormatException(
                $"Invalid Base64Url format: expected {sizeof(int)} bytes after decoding.");
        }

        _cipher.Decrypt(ciphertext, plaintext);

        return BinaryPrimitives.ReadInt32LittleEndian(plaintext);
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

    object ICryptoIdEncoder.Decode(ReadOnlySpan<char> urlEncodedBase64)
    {
        return Decode(urlEncodedBase64);
    }

    bool ICryptoIdEncoder.TryEncode(object id, Span<char> destination, out int charsWritten)
    {
        return TryEncode((int)id, destination, out charsWritten);
    }

    // -------------------------------------------------------------------------
    // Speck32/64
    // Parameters: n=16, m=4, T=22, α=7, β=2
    // Key schedule: K = (k3, k2, k1, k0), l[0..2] = (k1, k2, k3)
    // Round function: x = (rotr(x, α) + y) ⊕ k_i
    //                 y = rotl(y, β) ⊕ x
    // -------------------------------------------------------------------------
    /// <summary>
    /// Implements the Speck32/64 lightweight block cipher.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Speck32/64 parameters:
    /// - Word size: 16 bits (n=16)
    /// - Key words: 4 (m=4)
    /// - Rounds: 22 (T=22)
    /// - Right rotation (α): 7 bits
    /// - Left rotation (β): 2 bits
    /// </para>
    /// <para>
    /// The key schedule expands the 4 initial key words into 22 round keys using the specified rotations.
    /// Encryption and decryption use identical round operations but in forward and reverse order respectively.
    /// </para>
    /// <para>
    /// Reference: NSA Speck specification (https://eprint.iacr.org/2013/404.pdf), Algorithm 3
    /// </para>
    /// </remarks>
    internal sealed class Speck32_64
    {
        private const int Rounds = 22;
        private const int WordBits = 16;
        private const int Alpha = 7;
        private const int Beta = 2;
        private const int KeyWords = 4;
        private const int LLength = Rounds + KeyWords - 2; // 24

        // Mask to keep arithmetic results in 16 bits
        private const uint Mask16 = 0xFFFF;

        private readonly ushort[] _roundKeys = new ushort[Rounds];

        /// <summary>
        /// Initializes a new instance of the <see cref="Speck32_64"/> cipher.
        /// </summary>
        /// <param name="key">The 8-byte (64-bit) encryption key.</param>
        /// <exception cref="ArgumentException">Thrown when key length is not exactly 8 bytes.</exception>
        public Speck32_64(ReadOnlySpan<byte> key)
        {
            if (key.Length != 8)
            {
                throw new ArgumentException("Speck-32/64 key must be exactly 8 bytes.");
            }

            // Four 16-bit key words
            var k0 = BinaryPrimitives.ReadUInt16LittleEndian(key[..2]);
            var k1 = BinaryPrimitives.ReadUInt16LittleEndian(key.Slice(2, 2));
            var k2 = BinaryPrimitives.ReadUInt16LittleEndian(key.Slice(4, 2));
            var k3 = BinaryPrimitives.ReadUInt16LittleEndian(key.Slice(6, 2));

            Span<ushort> l = stackalloc ushort[LLength];
            l[0] = k1;
            l[1] = k2;
            l[2] = k3;

            _roundKeys[0] = k0;

            // Key schedule (Algorithm 3):
            //   l[i + m - 1] = (rotr(l[i], α) + k[i]) ⊕ i
            //   k[i + 1]     = rotl(k[i], β) ⊕ l[i + m - 1]
            //
            // Arithmetic is performed in uint, result is truncated to 16 bits via Mask16
            unchecked
            {
                for (var i = 0; i < Rounds - 1; i++)
                {
                    l[i + KeyWords - 1] = (ushort)(((RotR(l[i], Alpha) + _roundKeys[i]) & Mask16) ^ (uint)i);
                    _roundKeys[i + 1] = (ushort)(RotL(_roundKeys[i], Beta) ^ l[i + KeyWords - 1]);
                }
            }
        }

        /// <summary>
        /// Encrypts a 32-bit block (4 bytes) using the Speck32/64 algorithm.
        /// </summary>
        /// <param name="plaintext">The 4-byte plaintext block.</param>
        /// <param name="ciphertext">The output buffer for the 4-byte ciphertext block.</param>
        /// <exception cref="ArgumentException">Thrown when buffer sizes are not exactly 4 bytes.</exception>
        public void Encrypt(ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
        {
            ValidateBuffers(plaintext, ciphertext);

            var x = BinaryPrimitives.ReadUInt16LittleEndian(plaintext[..2]);
            var y = BinaryPrimitives.ReadUInt16LittleEndian(plaintext.Slice(2, 2));
            unchecked
            {
                for (var i = 0; i < Rounds; i++)
                {
                    x = (ushort)(((RotR(x, Alpha) + y) & Mask16) ^ _roundKeys[i]);
                    y = (ushort)(RotL(y, Beta) ^ x);
                }
            }

            BinaryPrimitives.WriteUInt16LittleEndian(ciphertext[..2], x);
            BinaryPrimitives.WriteUInt16LittleEndian(ciphertext.Slice(2, 2), y);
        }

        /// <summary>
        /// Decrypts a 32-bit block (4 bytes) using the Speck32/64 algorithm.
        /// </summary>
        /// <param name="ciphertext">The 4-byte ciphertext block.</param>
        /// <param name="plaintext">The output buffer for the 4-byte plaintext block.</param>
        /// <exception cref="ArgumentException">Thrown when buffer sizes are not exactly 4 bytes.</exception>
        public void Decrypt(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
        {
            ValidateBuffers(ciphertext, plaintext);

            var x = BinaryPrimitives.ReadUInt16LittleEndian(ciphertext[..2]);
            var y = BinaryPrimitives.ReadUInt16LittleEndian(ciphertext.Slice(2, 2));

            unchecked
            {
                for (var i = Rounds - 1; i >= 0; i--)
                {
                    y = (ushort)(RotR((uint)(x ^ y), Beta) & Mask16);
                    x = (ushort)(RotL(((uint)(x ^ _roundKeys[i]) - y) & Mask16, Alpha) & Mask16);
                }
            }

            BinaryPrimitives.WriteUInt16LittleEndian(plaintext[..2], x);
            BinaryPrimitives.WriteUInt16LittleEndian(plaintext.Slice(2, 2), y);
        }

        /// <summary>
        /// Performs a left rotation (circular left shift) on a 16-bit value.
        /// </summary>
        /// <param name="v">The value to rotate.</param>
        /// <param name="n">The number of bit positions to rotate left.</param>
        /// <returns>The rotated value, masked to 16 bits.</returns>
        /// <remarks>Cannot use BitOperations.RotateLeft while rotating ushort</remarks>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotL(uint v, int n)
        {
            unchecked
            {
                return ((v << n) | (v >> (WordBits - n))) & Mask16;
            }
        }

        /// <summary>
        /// Performs a right rotation (circular right shift) on a 16-bit value.
        /// </summary>
        /// <param name="v">The value to rotate.</param>
        /// <param name="n">The number of bit positions to rotate right.</param>
        /// <returns>The rotated value, masked to 16 bits.</returns>
        /// <remarks>Cannot use BitOperations.RotateRight while rotating ushort</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotR(uint v, int n)
        {
            unchecked
            {
                return ((v >> n) | (v << (WordBits - n))) & Mask16;
            }
        }

        /// <summary>
        /// Validates that input and output buffers are exactly 4 bytes (32 bits).
        /// </summary>
        /// <param name="input">The input buffer.</param>
        /// <param name="output">The output buffer.</param>
        /// <exception cref="ArgumentException">Thrown when buffer sizes are not exactly 4 bytes.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateBuffers(ReadOnlySpan<byte> input, Span<byte> output)
        {
            if (input.Length != sizeof(int) || output.Length != sizeof(int))
            {
                throw new ArgumentException("The size of the buffers must be exactly 4 bytes.");
            }
        }
    }
}

