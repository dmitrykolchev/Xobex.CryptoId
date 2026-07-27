// <copyright file="Speck32_64.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Xobex.Cryptography.Algo;

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
