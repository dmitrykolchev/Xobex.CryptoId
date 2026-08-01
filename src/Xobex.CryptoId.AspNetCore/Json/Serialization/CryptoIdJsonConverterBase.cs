// <copyright file="CryptoIdJsonConverterBase.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Base class for JSON converters that serialize and deserialize CryptoId values
/// using an encoder from the <see cref="CryptoIdRegistry"/>.
/// </summary>
/// <typeparam name="T">The identifier value type handled by the converter.</typeparam>
public abstract class CryptoIdJsonConverterBase<T> : JsonConverter<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoIdJsonConverterBase{T}"/> class
    /// that uses the default encoder registered for the identifier type.
    /// </summary>
    protected CryptoIdJsonConverterBase()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoIdJsonConverterBase{T}"/> class
    /// that uses the encoder registered under the specified registry key.
    /// </summary>
    /// <param name="registryKey">The registry key of the encoder to use.</param>
    protected CryptoIdJsonConverterBase(string registryKey)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(registryKey);
        Key = registryKey;
    }

    /// <summary>
    /// Gets the registry key of the encoder used for serialization,
    /// or null when the default encoder for the identifier type is used.
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// 
    /// </summary>
    [MemberNotNullWhen(true, nameof(Key))]
    public bool IsKeyed => Key != null;
}
