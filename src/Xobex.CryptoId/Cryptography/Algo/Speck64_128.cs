// <copyright file="Speck64_128.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Xobex.Cryptography.Algo;

// -------------------------------------------------------------------------
// Speck64/128 — canonical implementation per NSA specification
// https://eprint.iacr.org/2013/404.pdf , Algorithm 3
// Parameters: n=32 (word size), m=4 (key words), T=27 (rounds)
// -------------------------------------------------------------------------
/// <summary>
/// Implements the Speck64/128 lightweight block cipher.
/// </summary>
/// <remarks>
/// <para>
/// Speck64/128 parameters:
/// - Word size: 32 bits (n=32)
/// - Key words: 4 (m=4)
/// - Rounds: 27 (T=27)
/// - Right rotation (α): 8 bits
/// - Left rotation (β): 3 bits
/// </para>
/// <para>
/// The key schedule expands the 4 initial key words (128 bits) into 27 round keys.
/// Encryption and decryption use identical round operations but in forward and reverse order respectively.
/// </para>
/// <para>
/// Reference: NSA Speck specification (https://eprint.iacr.org/2013/404.pdf), Algorithm 3
/// </para>
/// </remarks>
internal sealed class Speck64_128
{
    private const int Rounds = 27;
    private const int Alpha = 8;    // right rotation for x in encrypt
    private const int Beta = 3;     // left rotation for y in encrypt

    // Specification: key schedule uses linear array l[0..T+m-3]
    // m=4 → l size is Rounds + m - 2 = 29
    private const int KeyWords = 4;
    private const int LLength = Rounds + KeyWords - 2; // 29

    private readonly uint[] _roundKeys = new uint[Rounds];

    /// <summary>
    /// Initializes a new instance of the <see cref="Speck64_128"/> cipher.
    /// </summary>
    /// <param name="key">The 16-byte (128-bit) encryption key.</param>
    /// <exception cref="ArgumentException">Thrown when key length is not exactly 16 bytes.</exception>
    public Speck64_128(ReadOnlySpan<byte> key)
    {
        if (key.Length != 16)
        {
            throw new ArgumentException("Speck-64/128 key must be exactly 16 bytes.");
        }

        // Read 4 key words in little-endian (platform-independent)
        // Per specification: K = (k_{m-1}, ..., k_1, k_0)
        // k_0 is the first word, l[0..m-2] = (k_1, k_2, k_3)
        var k0 = BinaryPrimitives.ReadUInt32LittleEndian(key[..4]);
        var k1 = BinaryPrimitives.ReadUInt32LittleEndian(key.Slice(4, 4));
        var k2 = BinaryPrimitives.ReadUInt32LittleEndian(key.Slice(8, 4));
        var k3 = BinaryPrimitives.ReadUInt32LittleEndian(key.Slice(12, 4));

        // l — array, not ring buffer
        // Initialization: l[0] = k1, l[1] = k2, l[2] = k3
        Span<uint> l = stackalloc uint[LLength];
        l[0] = k1;
        l[1] = k2;
        l[2] = k3;

        _roundKeys[0] = k0;

        // Key schedule (spec, Algorithm 3):
        //   l[i + m - 1] = (rotr(l[i], α) + k[i]) ⊕ i
        //   k[i + 1]     = rotl(k[i], β) ⊕ l[i + m - 1]
        unchecked
        {
            for (var i = 0; i < Rounds - 1; i++)
            {
                l[i + KeyWords - 1] = (BitOperations.RotateRight(l[i], Alpha) + _roundKeys[i]) ^ (uint)i;
                _roundKeys[i + 1] = BitOperations.RotateLeft(_roundKeys[i], Beta) ^ l[i + KeyWords - 1];
            }
        }
    }

    /// <summary>
    /// Encrypts a 64-bit block (8 bytes) using the Speck64/128 algorithm.
    /// </summary>
    /// <param name="plaintext">The 8-byte plaintext block.</param>
    /// <param name="ciphertext">The output buffer for the 8-byte ciphertext block.</param>
    /// <exception cref="ArgumentException">Thrown when buffer sizes are not exactly 8 bytes.</exception>
    public void Encrypt(ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        ValidateBuffers(plaintext, ciphertext);

        // Speck: block = (x, y), x — left word (hi), y — right
        // plaintext[0..3] → x, plaintext[4..7] → y
        var x = BinaryPrimitives.ReadUInt32LittleEndian(plaintext[..4]);
        var y = BinaryPrimitives.ReadUInt32LittleEndian(plaintext.Slice(4, 4));

        // Round function: x = (rotr(x, α) + y) ⊕ k_i
        //                 y = rotl(y, β) ⊕ x
        unchecked
        {
            for (var i = 0; i < Rounds; i++)
            {
                x = (BitOperations.RotateRight(x, Alpha) + y) ^ _roundKeys[i];
                y = BitOperations.RotateLeft(y, Beta) ^ x;
            }
        }

        BinaryPrimitives.WriteUInt32LittleEndian(ciphertext[..4], x);
        BinaryPrimitives.WriteUInt32LittleEndian(ciphertext.Slice(4, 4), y);
    }

    /// <summary>
    /// Decrypts a 64-bit block (8 bytes) using the Speck64/128 algorithm.
    /// </summary>
    /// <param name="ciphertext">The 8-byte ciphertext block.</param>
    /// <param name="plaintext">The output buffer for the 8-byte plaintext block.</param>
    /// <exception cref="ArgumentException">Thrown when buffer sizes are not exactly 8 bytes.</exception>
    public void Decrypt(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        ValidateBuffers(ciphertext, plaintext);

        var x = BinaryPrimitives.ReadUInt32LittleEndian(ciphertext[..4]);
        var y = BinaryPrimitives.ReadUInt32LittleEndian(ciphertext.Slice(4, 4));

        unchecked
        {
            // Inverse of round function in reverse order of rounds
            for (var i = Rounds - 1; i >= 0; i--)
            {
                y = BitOperations.RotateRight(x ^ y, Beta);
                x = BitOperations.RotateLeft((x ^ _roundKeys[i]) - y, Alpha);
            }
        }

        BinaryPrimitives.WriteUInt32LittleEndian(plaintext[..4], x);
        BinaryPrimitives.WriteUInt32LittleEndian(plaintext.Slice(4, 4), y);
    }

    /// <summary>
    /// Validates that input and output buffers are exactly 8 bytes (64 bits).
    /// </summary>
    /// <param name="input">The input buffer.</param>
    /// <param name="output">The output buffer.</param>
    /// <exception cref="ArgumentException">Thrown when buffer sizes are not exactly 8 bytes.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateBuffers(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length != sizeof(long) || output.Length != sizeof(long))
        {
            throw new ArgumentException("The size of the buffers must be exactly 8 bytes.");
        }
    }
}
