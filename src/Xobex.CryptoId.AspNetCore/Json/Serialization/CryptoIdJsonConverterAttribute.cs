// <copyright file="CryptoIdJsonConverterAttribute.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Configures JSON serialization for CryptoId values using the encoder registered
/// under the specified key.
/// </summary>
public class CryptoIdJsonConverterAttribute : JsonConverterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoIdJsonConverterAttribute"/> class.
    /// </summary>
    /// <param name="key">The registry key of the encoder to use for serialization.</param>
    public CryptoIdJsonConverterAttribute(string key)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(key);
        Key = key;
    }

    /// <summary>
    /// Gets the registry key of the encoder used for serialization.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Creates the JSON converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type being serialized.</param>
    /// <returns>The converter for the specified type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="typeToConvert"/> is not a supported type.
    /// </exception>
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
        throw new InvalidOperationException($"not supported type {typeToConvert}");
    }
}
