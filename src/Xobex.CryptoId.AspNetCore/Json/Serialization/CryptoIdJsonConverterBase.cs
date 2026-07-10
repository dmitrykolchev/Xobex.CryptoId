// <copyright file="CryptoIdConverterBase.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CryptoIdJsonConverterBase<T>: JsonConverter<T>
{
    /// <summary>
    /// 
    /// </summary>
    protected CryptoIdJsonConverterBase()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="registryKey"></param>
    protected CryptoIdJsonConverterBase(string registryKey)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(registryKey);
        Key = registryKey;
    }

    /// <summary>
    /// Registry key
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// 
    /// </summary>
    [MemberNotNullWhen(true, nameof(Key))]
    public bool IsKeyed => Key != null;
}
