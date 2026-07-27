// <copyright file="HashProvider.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;

namespace Xobex.Cryptography.Algo;

internal static class HashProvider
{
    /// <summary>
    /// Computes a 64-bit non-cryptographic FNV-1a hash.
    /// The algorithm runs in O(N) bytes, for 8 bytes this is only 8 XOR/MUL iterations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeFnv1a64(ReadOnlySpan<byte> data)
    {
        var hash = 14695981039346656037UL; // FNV offset basis
        foreach (var b in data)
        {
            hash ^= b;
            hash *= 1099511628211UL; // FNV prime
        }
        return hash;
    }
}
