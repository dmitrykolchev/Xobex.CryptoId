// <copyright file="CryptoIdFactory.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Cryptography.Abstractions;

namespace Xobex.Cryptography;

/// <summary>
/// Factory class for creating cryptographic identifier encoders.
/// </summary>
/// <remarks>
/// This factory provides a convenient way to instantiate different encoder implementations
/// based on the desired algorithm and data type. It handles algorithm-to-encoder routing.
/// </remarks>
public class CryptoIdFactory
{
    /// <summary>
    /// Creates a cryptographic identifier encoder for the specified algorithm.
    /// </summary>
    /// <param name="algorithm">The cryptographic algorithm to use.</param>
    /// <param name="key">The cryptographic key material.</param>
    /// <param name="salt">Salt for HKDF key derivation. Must be stable across restarts so that previously issued IDs remain decodable.</param>
    /// <returns>An encoder instance implementing <see cref="ICryptoIdEncoder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="salt"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the algorithm is not supported.</exception>
    public static ICryptoIdEncoder Create(IdCipherAlgorithm algorithm, string key, byte[]? salt = null)
    {
        ArgumentNullException.ThrowIfNull(salt);
        ICryptoIdEncoder result = algorithm switch
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IdCipherAlgorithm.AesGcm => new AesGcmCryptoIdEncoder(key, salt),
#pragma warning restore CS0618 // Type or member is obsolete
            IdCipherAlgorithm.DeterministicChaCha20Poly1305 => new DeterministicChaCha20Poly1305CryptoIdEncoder(key, salt),
            IdCipherAlgorithm.DeterministicAesGcm => new DeterministicAesGcmCryptoIdEncoder(key, salt),
            IdCipherAlgorithm.CompactDeterministicAes => new CompactDeterministicAesCryptoIdEncoder(key, salt),
            IdCipherAlgorithm.Speck64_128 => new Speck64128CryptoIdEncoder(key, salt),
            IdCipherAlgorithm.Speck32_64 => new Speck3264CryptoIdEncoder(key, salt),
            IdCipherAlgorithm.Skip32 => new Skip32CryptoIdEncoder(key, salt),
            _ => throw new ArgumentException("unsupported algorithm", nameof(algorithm))
        };
        return result;
    }

    /// <summary>
    /// Creates a cryptographic identifier encoder for the specified algorithm and data type.
    /// </summary>
    /// <typeparam name="T">The data type of identifiers to encode. Supported types: <see cref="long"/> and <see cref="int"/>.</typeparam>
    /// <param name="algorithm">The cryptographic algorithm to use.</param>
    /// <param name="key">
    /// The cryptographic key material (e.g., password, API key, or random string).
    /// This will be processed through HKDF-SHA256 for key derivation.
    /// </param>
    /// <param name="salt">Salt for HKDF key derivation. Must be stable across restarts so that previously issued IDs remain decodable.</param>
    /// <returns>An encoder instance implementing <see cref="ICryptoIdEncoder{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="salt"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the algorithm is not supported for the specified type <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when <typeparamref name="T"/> is not a supported type (must be <see cref="long"/> or <see cref="int"/>).
    /// </exception>
    public static ICryptoIdEncoder<T> Create<T>(IdCipherAlgorithm algorithm, string key, byte[]? salt = null)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(salt);
        ICryptoIdEncoder result;
        if (typeof(T) == typeof(long))
        {
            result = algorithm switch
            {
#pragma warning disable CS0618 // Type or member is obsolete
                IdCipherAlgorithm.AesGcm => new AesGcmCryptoIdEncoder(key, salt),
#pragma warning restore CS0618 // Type or member is obsolete
                IdCipherAlgorithm.DeterministicChaCha20Poly1305 => new DeterministicChaCha20Poly1305CryptoIdEncoder(key, salt),
                IdCipherAlgorithm.DeterministicAesGcm => new DeterministicAesGcmCryptoIdEncoder(key, salt),
                IdCipherAlgorithm.CompactDeterministicAes => new CompactDeterministicAesCryptoIdEncoder(key, salt),
                IdCipherAlgorithm.Speck64_128 => new Speck64128CryptoIdEncoder(key, salt),
                _ => throw new ArgumentException("unsupported algorithm", nameof(algorithm))
            };
        }
        else if (typeof(T) == typeof(int))
        {
            result = algorithm switch
            {
                IdCipherAlgorithm.Speck32_64 => new Speck3264CryptoIdEncoder(key, salt),
                IdCipherAlgorithm.Skip32 => new Skip32CryptoIdEncoder(key, salt),
                _ => throw new ArgumentException("unsupported algorithm", nameof(algorithm))
            };
        }
        else
        {
            throw new NotSupportedException("unsupported data type");
        }
        return (ICryptoIdEncoder<T>)result;
    }
}
