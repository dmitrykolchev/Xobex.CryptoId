// <copyright file="CryptoIdRegistry.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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

    internal static ConcurrentDictionary<string, ICryptoIdEncoder> _registry = [];

    /// <summary>
    /// Registers the encoder for int and long types. This method should be
    /// called during the DI container setup to ensure that the encoder is
    /// available for all instances of the converter.
    /// </summary>
    /// <param name="encoder">The encoder to register as the default encoder for <see cref="int"/> identifiers.</param>
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
    /// <param name="encoder">The encoder to register as the default encoder for <see cref="long"/> identifiers.</param>
    public static void Register(ICryptoIdEncoder<long> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _int64encoder = encoder;
    }

    /// <summary>
    /// Attempts to register an encoder under the specified key. Registration is skipped
    /// if the key is already present.
    /// </summary>
    /// <param name="key">The unique key for the encoder.</param>
    /// <param name="encoder">The encoder to register.</param>
    /// <returns>true if the encoder was registered; otherwise, false if the key already exists.</returns>
    public static bool TryRegister(string key, ICryptoIdEncoder encoder)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(encoder);
        return _registry.TryAdd(key, encoder);
    }

    /// <summary>
    /// Registers an encoder under the specified key.
    /// </summary>
    /// <param name="key">The unique key for the encoder.</param>
    /// <param name="encoder">The encoder to register.</param>
    /// <exception cref="InvalidOperationException">Thrown when the key is already registered.</exception>
    public static void Register(string key, ICryptoIdEncoder encoder)
    {
        if(!TryRegister(key, encoder))
        {
            throw new InvalidOperationException($"Cannot register encoder with key {key}");
        }
    }

    /// <summary>
    /// Attempts to retrieve an encoder by its key.
    /// </summary>
    /// <param name="key">The key of the encoder to retrieve.</param>
    /// <param name="value">When this method returns, contains the encoder associated with the key, if found.</param>
    /// <returns>true if the encoder was found; otherwise, false.</returns>
    public static bool TryGet(string key, [NotNullWhen(true)] out ICryptoIdEncoder? value)
    {
        return _registry.TryGetValue(key, out value);
    }

    /// <summary>
    /// Retrieves an encoder by its key.
    /// </summary>
    /// <param name="key">The key of the encoder to retrieve.</param>
    /// <returns>The registered encoder.</returns>
    /// <exception cref="ArgumentException">Thrown when no encoder is registered under the specified key.</exception>
    public static ICryptoIdEncoder Get(string key)
    {
        if (TryGet(key, out var result))
        {
            return result;
        }
        throw new ArgumentException($"encoder with key = ({key}) was not registered");
    }

    /// <summary>
    /// Retrieves a strongly-typed encoder by its key.
    /// </summary>
    /// <typeparam name="T">The identifier type handled by the encoder.</typeparam>
    /// <param name="key">The key of the encoder to retrieve.</param>
    /// <returns>The registered encoder cast to <see cref="ICryptoIdEncoder{T}"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when no encoder is registered under the specified key.</exception>
    /// <exception cref="InvalidCastException">Thrown when the registered encoder does not handle type <typeparamref name="T"/>.</exception>
    public static ICryptoIdEncoder<T> Get<T>(string key) where T: struct
    {
        if (TryGet(key, out var result))
        {
            return (ICryptoIdEncoder<T>)result;
        }
        throw new ArgumentException($"encoder with key = ({key}) was not registered");
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
