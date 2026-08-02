// <copyright file="Base64Url.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

#if NET8_0
// System.Buffers.Text.Base64Url was introduced in .NET 9. This polyfill provides the
// subset of the API used by the encoders for net8.0 builds only.
using System;

namespace System.Buffers.Text;

internal static class Base64Url
{
    public static string EncodeToString(ReadOnlySpan<byte> source)
    {
        var length = (source.Length + 2) / 3 * 4;
        var buffer = length <= 1024 ? stackalloc char[1024] : new char[length];
        TryEncodeToChars(source, buffer, out var written);
        return new string(buffer[..written]);
    }

    public static bool TryEncodeToChars(ReadOnlySpan<byte> source, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;

        var remainder = source.Length % 3;
        var padding = remainder == 0 ? 0 : 3 - remainder;
        var encodedLength = ((source.Length + 2) / 3 * 4) - padding;

        if (destination.Length < encodedLength)
        {
            return false;
        }

        if (!Convert.TryToBase64Chars(source, destination, out var written))
        {
            return false;
        }

        var index = 0;
        for (var i = 0; i < encodedLength; i++)
        {
            var c = destination[i];
            destination[index++] = c switch
            {
                '+' => '-',
                '/' => '_',
                _ => c,
            };
        }

        charsWritten = index;
        return true;
    }

    public static bool TryDecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (source.Length == 0)
        {
            return true;
        }

        var remainder = source.Length % 4;
        if (remainder == 1)
        {
            return false;
        }

        var padding = remainder == 0 ? 0 : 4 - remainder;
        var paddedLength = source.Length + padding;

        var padded = paddedLength <= 1024 ? stackalloc char[1024] : new char[paddedLength];
        for (var i = 0; i < source.Length; i++)
        {
            var c = source[i];
            padded[i] = c switch
            {
                '-' => '+',
                '_' => '/',
                _ => c,
            };
        }

        for (var i = source.Length; i < paddedLength; i++)
        {
            padded[i] = '=';
        }

        if (!Convert.TryFromBase64Chars(padded[..paddedLength], destination, out var written))
        {
            return false;
        }

        bytesWritten = written;
        return true;
    }
}
#endif
