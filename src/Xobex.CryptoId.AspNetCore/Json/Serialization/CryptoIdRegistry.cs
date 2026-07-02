// <copyright file="CryptoIdRegistry.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// A static registry for managing cryptographic ID encoders for int and long types.
/// </summary>
public static class CryptoIdRegistry
{
    /// <summary>
    /// Initialized while DI container is being built. This is a static field to ensure that
    /// the encoder is shared across all instances of the converter.
    /// </summary>
    internal static ICryptoIdEncoder<int>? _int32encoder;

    /// <summary>
    /// Initialized while DI container is being built. This is a static field to ensure that
    /// the encoder is shared across all instances of the converter.
    /// </summary>
    internal static ICryptoIdEncoder<long>? _int64encoder;

    /// <summary>
    /// Registers the encoder for int and long types. This method should be
    /// called during the DI container setup to ensure that the encoder is
    /// available for all instances of the converter.
    /// </summary>
    /// <param name="encoder"></param>
    public static void Register(ICryptoIdEncoder<int> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _int32encoder = encoder;
    }

    /// <summary>
    /// Registers the encoder for long types. This method should be
    /// called during the DI container setup to ensure that the encoder is
    /// available for all instances of the converter.
    /// </summary>
    /// <param name="encoder"></param>
    public static void Register(ICryptoIdEncoder<long> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _int64encoder = encoder;
    }

    /// <summary>
    /// Gets the registered encoder for int types. Throws an exception if the encoder is not registered.
    /// </summary>
    public static ICryptoIdEncoder<int> Int32Encoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_int32encoder is null)
            {
                throw new InvalidOperationException("Int32 encoder is not registered. Please register it during DI container setup.");
            }
            return _int32encoder;
        }
    }

    /// <summary>
    /// Gets the registered encoder for long types. Throws an exception if the encoder is not registered.
    /// </summary>
    public static ICryptoIdEncoder<long> Int64Encoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_int64encoder is null)
            {
                throw new InvalidOperationException("Int64 encoder is not registered. Please register it during DI container setup.");
            }
            return _int64encoder;
        }
    }
}
