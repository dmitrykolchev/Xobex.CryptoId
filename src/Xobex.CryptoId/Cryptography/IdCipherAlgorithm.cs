// <copyright file="IdCipherAlgorithm.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

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
    Speck64_128,

    /// <summary>
    /// Skip 32 lightweight block cipher.
    /// Suitable for encrypting 32-bit (int) identifiers.
    /// </summary>
    Skip32,

    /// <summary>
    /// ChaCha20-Poly1305 authenticated encryption algorithm.
    /// Suitable for encrypting 64-bit (long) identifiers with authentication.
    /// </summary>
    ChaCha20Poly1305,

    /// <summary>
    /// Deterministic ChaCha20-Poly1305 authenticated encryption algorithm with deterministic nonce generation.
    /// </summary>
    DeterministicChaCha20Poly1305,

    /// <summary>
    /// Deterministic AES-GCM (Advanced Encryption Standard with Galois/Counter Mode) with
    /// deterministic nonce generation.
    /// </summary>
    DeterministicAesGcm,

    /// <summary>
    /// Compact deterministic AES (Advanced Encryption Standard) with deterministic nonce generation.
    /// </summary>
    CompactDeterministicAes,
}

