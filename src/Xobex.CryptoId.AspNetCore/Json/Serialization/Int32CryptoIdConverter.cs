// <copyright file="Int32CryptoIdConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the Int32CryptoId type, enabling
/// </summary>
internal class Int32CryptoIdConverter : JsonConverter<Int32CryptoId>
{
    /// <summary>
    /// Initializes a new instance of the Int32CryptoIdConverter class.
    /// </summary>
    public Int32CryptoIdConverter()
    {
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
        return (Int32CryptoId)CryptoIdRegistry.Int32Encoder.Decode(reader.GetString());
    }

    /// <summary>
    /// Writes a JSON representation of an Int32CryptoId object.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Int32CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(CryptoIdRegistry.Int32Encoder.Encode(value.Value));
    }
}
