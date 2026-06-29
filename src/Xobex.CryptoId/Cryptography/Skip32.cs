// <copyright file="Skip32.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;

namespace Xobex.Cryptography;

/// <summary>
/// Skip32 — 32-битный блочный шифр (Format-Preserving Encryption: uint32 → uint32).
/// Реализует алгоритм Unruh (1998) на основе F-таблицы Skipjack (NSA, 1998).
///
/// Применение: обфускация последовательных ID в публичных URL/API.
/// Не использовать для криптографической защиты — 32-битный блок уязвим
/// к birthday attack при ~2^16 зашифрованных значениях.
/// </summary>
internal sealed class Skip32
{
    /// <summary>
    /// Каноническая F-таблица Skipjack (NSA, 1998).
    /// Источник: официальная спецификация SKIPJACK.
    /// </summary>
    private static readonly byte[] FTable =
    [
        0xa3, 0xd7, 0x09, 0x83, 0xf8, 0x48, 0xf6, 0xf4, 0xb3, 0x21, 0x15, 0x78, 0x99, 0xb1, 0xaf, 0xf9,
        0xe7, 0x2d, 0x4d, 0x8a, 0xce, 0x4c, 0xca, 0x2e, 0x52, 0x95, 0xd9, 0x1e, 0x4e, 0x38, 0x44, 0x28,
        0x0a, 0xdf, 0x02, 0xa0, 0x17, 0xf1, 0x60, 0x68, 0x12, 0xb7, 0x7a, 0xc3, 0xe9, 0xfa, 0x3d, 0x53,
        0x96, 0x84, 0x6b, 0xba, 0xf2, 0x63, 0x9a, 0x19, 0x7c, 0xae, 0xe5, 0xf5, 0xf7, 0x16, 0x6a, 0xa2,
        0x39, 0xb6, 0x7b, 0x0f, 0xc1, 0x93, 0x81, 0x1b, 0xee, 0xb4, 0x1a, 0xea, 0xd0, 0x91, 0x2f, 0xb8,
        0x55, 0xb9, 0xda, 0x85, 0x3f, 0x41, 0xbf, 0xe0, 0x5a, 0x58, 0x80, 0x5f, 0x66, 0x0b, 0xd8, 0x90,
        0x35, 0xd5, 0xc0, 0xa7, 0x33, 0x06, 0x65, 0x69, 0x45, 0x00, 0x94, 0x56, 0x6d, 0x98, 0x9b, 0x76,
        0x97, 0xfc, 0xb2, 0xc2, 0xb0, 0xfe, 0xdb, 0x20, 0xe1, 0xeb, 0xd6, 0xe4, 0xdd, 0x47, 0x4a, 0x1d,
        0x42, 0xed, 0x9e, 0x6e, 0x49, 0x3c, 0xcd, 0x43, 0x27, 0xd2, 0x07, 0xd4, 0xde, 0xc7, 0x67, 0x18,
        0x89, 0xcb, 0x30, 0x1f, 0x8d, 0xc6, 0x8f, 0xaa, 0xc8, 0x74, 0xdc, 0xc9, 0x5d, 0x5c, 0x31, 0xa4,
        0x70, 0x88, 0x61, 0x2c, 0x9f, 0x0d, 0x2b, 0x87, 0x50, 0x82, 0x54, 0x64, 0x26, 0x7d, 0x03, 0x40,
        0x34, 0x4b, 0x1c, 0x73, 0xd1, 0xc4, 0xfd, 0x3b, 0xcc, 0xfb, 0x7f, 0xab, 0xe6, 0x3e, 0x5b, 0xa5,
        0xad, 0x04, 0x23, 0x9c, 0x14, 0x51, 0x22, 0xf0, 0x29, 0x79, 0x71, 0x7e, 0xff, 0x8c, 0x0e, 0xe2,
        0x0c, 0xef, 0xbc, 0x72, 0x75, 0x6f, 0x37, 0xa1, 0xec, 0xd3, 0x8e, 0x62, 0x8b, 0x86, 0x10, 0xe8,
        0x08, 0x77, 0x11, 0xbe, 0x92, 0x4f, 0x24, 0xc5, 0x32, 0x36, 0x9d, 0xcf, 0xf3, 0xa6, 0xbb, 0xac,
        0x5e, 0x6c, 0xa9, 0x13, 0x57, 0x25, 0xb5, 0xe3, 0xbd, 0xa8, 0x3a, 0x01, 0x05, 0x59, 0x2a, 0x46
    ];

    private const int Rounds = 24;

    [InlineArray(10)]
    private struct KeyBuffer { private byte _element0; }

    private readonly KeyBuffer _key;

    static Skip32()
    {
        if (FTable.Distinct().Count() != 256)
        {
            throw new InvalidOperationException("FTable должна быть перестановкой 0..255.");
        }

        if (FTable[0x7a] != 0xd6)
        {
            throw new InvalidOperationException("FTable не прошла контрольную проверку: F(0x7a) должно быть 0xd6.");
        }
    }

    /// <param name="key">Ключ: ровно 10 байт (80 бит)</param>
    public Skip32(ReadOnlySpan<byte> key)
    {
        if (key.Length != 10)
        {
            throw new ArgumentException("Ключ Skip32 должен состоять ровно из 10 байт.", nameof(key));
        }
        key.CopyTo(_key);
    }

    /// <summary>
    /// Зашифровать 32-битное значение. Encrypt(Decrypt(x)) == x для любого x.
    /// </summary>
    public uint Encrypt(uint value)
    {
        var wl = (ushort)(value >> 16);
        var wr = (ushort)(value & 0xFFFF);

        for (var k = 0; k < Rounds; k++)
        {
            var round = k + 1;
            if ((round & 1) == 1)
            {
                wr = (ushort)(wr ^ G(wl, k) ^ round);
            }
            else
            {
                wl = (ushort)(wl ^ G(wr, k) ^ round);
            }
        }

        return ((uint)wl << 16) | wr;
    }

    /// <summary>
    /// Расшифровать 32-битное значение. Decrypt(Encrypt(x)) == x для любого x.
    /// </summary>
    public uint Decrypt(uint value)
    {
        var wl = (ushort)(value >> 16);
        var wr = (ushort)(value & 0xFFFF);

        for (var k = Rounds - 1; k >= 0; k--)
        {
            var round = k + 1;
            if ((round & 1) == 1)
            {
                wr = (ushort)(wr ^ G(wl, k) ^ round);
            }
            else
            {
                wl = (ushort)(wl ^ G(wr, k) ^ round);
            }
        }

        return ((uint)wl << 16) | wr;
    }

    /// <summary>
    /// G-перестановка: 16→16 бит псевдослучайная функция.
    ///   g3 = F[g2 ^ key[(4k+0) % 10]] ^ g1
    ///   g4 = F[g3 ^ key[(4k+1) % 10]] ^ g2
    ///   g5 = F[g4 ^ key[(4k+2) % 10]] ^ g3
    ///   g6 = F[g5 ^ key[(4k+3) % 10]] ^ g4
    ///   result = (g5 &lt;&lt; 8) | g6
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort G(ushort w, int k)
    {
        var g1 = (byte)(w >> 8);
        var g2 = (byte)(w & 0xFF);

        var g3 = (byte)(FTable[g2 ^ _key[((k * 4) + 0) % 10]] ^ g1);
        var g4 = (byte)(FTable[g3 ^ _key[((k * 4) + 1) % 10]] ^ g2);
        var g5 = (byte)(FTable[g4 ^ _key[((k * 4) + 2) % 10]] ^ g3);
        var g6 = (byte)(FTable[g5 ^ _key[((k * 4) + 3) % 10]] ^ g4);

        return (ushort)((g5 << 8) | g6);
    }
}
