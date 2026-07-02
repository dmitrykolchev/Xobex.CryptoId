// <copyright file="CryptoIdOptions.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Cryptography;

namespace Xobex.CryptoId.DependencyInjection;

/// <summary>
/// Options for configuring the CryptoId services, including the cipher algorithms for Int32 and Int64 IDs
/// and the secret key used for encoding and decoding.
/// </summary>
public sealed class CryptoIdOptions
{
    /// <summary>
    /// Gets or sets the cipher algorithm to be used for encoding and decoding Int32 IDs. The default is Speck32_64.
    /// </summary>
    public IdCipherAlgorithm Int32Algorithm { get; set; } = IdCipherAlgorithm.Speck32_64;
    /// <summary>
    /// Gets or sets the cipher algorithm to be used for encoding and decoding Int64 IDs. The default is Speck64_128.
    /// </summary>
    public IdCipherAlgorithm Int64Algorithm { get; set; } = IdCipherAlgorithm.Speck64_128;
    /// <summary>
    /// Gets or sets the secret key used for encoding and decoding IDs. If not provided, a random secret will be generated.
    /// </summary>
    public string Secret { get; set; } = null!;
}
