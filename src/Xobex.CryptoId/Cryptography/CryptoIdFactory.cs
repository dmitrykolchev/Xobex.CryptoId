// <copyright file="IdCipher.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Cryptography.Abstractions;

namespace Xobex.Cryptography;

/// <summary>
/// Specifies the cryptographic algorithm used for encoding and decoding identifiers.
/// </summary>
public enum IdCipherAlgorithm
{
    /// <summary>
    /// AES-GCM (Advanced Encryption Standard with Galois/Counter Mode).
    /// Suitable for encrypting 64-bit (long) identifiers with authentication.
    /// </summary>
    AesGcm,

    /// <summary>
    /// Speck 32/64 lightweight block cipher.
    /// Suitable for encrypting 32-bit (int) identifiers.
    /// </summary>
    Speck32_64,

    /// <summary>
    /// Speck 64/128 lightweight block cipher.
    /// Suitable for encrypting 64-bit (long) identifiers.
    /// </summary>
    Speck64_128
}

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
    private static readonly byte[] DefaultSalt = Convert.FromHexString("6b4e3a9f1c8d2e7b0a5f4c3d9e1b8a2f");

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
        object result;
        if (typeof(T) == typeof(long))
        {
            result = algorithm switch
            {
                IdCipherAlgorithm.AesGcm => new AesCryptoIdEncoder(key, salt ?? DefaultSalt),
                IdCipherAlgorithm.Speck64_128 => new Speck64128CryptoIdEncoder(key, salt ?? DefaultSalt),
                _ => throw new ArgumentException("unsupported algorithm", nameof(algorithm))
            };
        }
        else if (typeof(T) == typeof(int))
        {
            result = algorithm switch
            {
                IdCipherAlgorithm.Speck32_64 => new Speck3264CryptoIdEncoder(key, salt ?? DefaultSalt),
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
