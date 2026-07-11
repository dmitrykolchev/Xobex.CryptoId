// <copyright file="Int64CryptoIdConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the Int64CryptoId type, enabling
/// </summary>
public sealed class Int64CryptoIdConverter : CryptoIdJsonConverterBase<Int64CryptoId>
{
    private readonly ICryptoIdEncoder<long> _encoder;

    /// <summary>
    /// Initializes a new instance of the Int64CryptoIdConverter class.
    /// </summary>
    public Int64CryptoIdConverter()
    {
        _encoder = CryptoIdRegistry.Int64Encoder ?? throw new InvalidOperationException("encoder not registered");
    }

    /// <summary>
    /// Initializes a new instance of the Int64CryptoIdConverter class.
    /// </summary>
    /// <param name="registryKey"></param>
    public Int64CryptoIdConverter(string registryKey): base(registryKey)
    {
        _encoder = (ICryptoIdEncoder<long>)CryptoIdRegistry.Get(registryKey);
    }

    /// <summary>
    /// Reads and converts the JSON representation of an Int64CryptoId object.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override Int64CryptoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Int64CryptoId)_encoder.Decode(reader.GetString());
    }

    /// <summary>
    /// Writes a Int64CryptoId object as a JSON string.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Int64CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder.Encode(value.Value));
    }
}
