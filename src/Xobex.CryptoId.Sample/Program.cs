// <copyright file="Program.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Cryptography;

namespace Xobex.CryptoId.Sample;

internal class Program
{
    static void Main(string[] args)
    {
        var longId = long.MaxValue;
        var intId = int.MaxValue;

        foreach (var algorithm in Enum.GetValues<IdCipherAlgorithm>())
        {
            var encoder = CryptoIdFactory.Create(algorithm, "Hello World!");
            if (encoder.IdType == typeof(long))
            {
                var encodedString = encoder.Encode(longId);
                var result = encoder.Decode(encodedString);
                if(!longId.Equals(result))
                {
                    throw new InvalidOperationException("Decoded value does not match the original long ID.");
                }
                Console.WriteLine($"| {encoder.GetType().Name[..^15],-30} | {result,20}L | {encodedString,-48} | {encodedString.Length,2} |");
            }
            else
            {
                var encodedString = encoder.Encode(intId);
                var result = encoder.Decode(encodedString);
                if (!intId.Equals(result))
                {
                    throw new InvalidOperationException("Decoded value does not match the original int ID.");
                }
                Console.WriteLine($"| {encoder.GetType().Name[..^15],-30} | {result,20}  | {encodedString,-48} | {encodedString.Length,2} |");
            }
        }
    }
}
