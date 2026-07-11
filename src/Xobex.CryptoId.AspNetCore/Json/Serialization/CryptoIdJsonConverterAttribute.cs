// <copyright file="CryptoIdJsonConverterAttribute.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// 
/// </summary>
public class CryptoIdJsonConverterAttribute : JsonConverterAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    public CryptoIdJsonConverterAttribute(string key)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(key);
        Key = key;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        if (typeToConvert == typeof(int))
        {
            return new Int32CryptoIdConverter(Key);
        }
        else if(typeToConvert == typeof(long))
        {
            return new Int64CryptoIdConverter(Key);
        }
        else if (typeToConvert == typeof(Int32CryptoId))
        {
            return new Int32CryptoIdConverter(Key);
        }
        else if(typeToConvert == typeof(Int64CryptoId))
        {
            return new Int64CryptoIdConverter(Key);
        }
        throw new InvalidOperationException($"not supperted type {typeToConvert}");
    }
}
