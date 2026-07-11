// <copyright file="Int32CryptoIdConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the Int32CryptoId type, enabling
/// </summary>
public sealed class Int32CryptoIdConverter : CryptoIdJsonConverterBase<Int32CryptoId>
{
    private readonly ICryptoIdEncoder<int> _encoder;
    /// <summary>
    /// Initializes a new instance of the Int32CryptoIdConverter class.
    /// </summary>
    public Int32CryptoIdConverter()
    {
        _encoder = CryptoIdRegistry.Int32Encoder ?? throw new InvalidOperationException("encoder not registered");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="registryKey"></param>
    public Int32CryptoIdConverter(string registryKey) : base(registryKey)
    {
        _encoder = (ICryptoIdEncoder<int>)CryptoIdRegistry.Get(registryKey);
    }

    /// <summary>
    /// Reads and converts the JSON representation of an Int32CryptoId object.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override Int32CryptoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Int32CryptoId)_encoder.Decode(reader.GetString());
    }

    /// <summary>
    /// Writes a JSON representation of an Int32CryptoId object.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Int32CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder.Encode(value.Value));
    }
}
