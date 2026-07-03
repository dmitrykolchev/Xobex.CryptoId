// <copyright file="CryptoIdFactory.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Security.Cryptography;
using Xobex.Cryptography.Abstractions;

namespace Xobex.Cryptography;

/// <summary>
/// Factory class for creating cryptographic identifier encoders.
/// </summary>
/// <remarks>
/// This factory provides a convenient way to instantiate different encoder implementations
/// based on the desired algorithm and data type. It handles algorithm-to-encoder routing
/// and includes sensible defaults for configuration like salt.
/// </remarks>
public class CryptoIdFactory
{
    /// <summary>
    /// Default salt value for HKDF key derivation.
    /// In production environments, replace this with a unique, cryptographically random value
    /// specific to your deployment.
    /// </summary>
    public static readonly byte[] DefaultSalt;

    static CryptoIdFactory()
    {
        RandomNumberGenerator.Fill(DefaultSalt = new byte[16]);
    }

    /// <summary>
    /// Creates a cryptographic identifier encoder for the specified algorithm.
    /// </summary>
    /// <param name="algorithm">The cryptographic algorithm to use.</param>
    /// <param name="key">The cryptographic key material.</param>
    /// <param name="salt">Optional salt for HKDF key derivation.</param>
    /// <returns>An encoder instance implementing <see cref="ICryptoIdEncoder"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the algorithm is not supported.</exception>
    public static ICryptoIdEncoder Create(IdCipherAlgorithm algorithm, string key, byte[]? salt = null)
    {
        salt ??= DefaultSalt;
        ICryptoIdEncoder result = algorithm switch
        {
            IdCipherAlgorithm.AesGcm => new AesGcmCryptoIdEncoder(key, salt),
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
    /// <param name="salt">
    /// Optional salt for HKDF key derivation. If null, the default salt is used.
    /// In production, provide a unique salt specific to your deployment.
    /// </param>
    /// <returns>An encoder instance implementing <see cref="ICryptoIdEncoder{T}"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the algorithm is not supported for the specified type <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when <typeparamref name="T"/> is not a supported type (must be <see cref="long"/> or <see cref="int"/>).
    /// </exception>
    public static ICryptoIdEncoder<T> Create<T>(IdCipherAlgorithm algorithm, string key, byte[]? salt = null)
        where T : struct
    {
        ICryptoIdEncoder result;
        salt ??= DefaultSalt;
        if (typeof(T) == typeof(long))
        {
            result = algorithm switch
            {
                IdCipherAlgorithm.AesGcm => new AesGcmCryptoIdEncoder(key, salt),
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
