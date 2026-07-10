// <copyright file="Int64JsonConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the Int64CryptoId type, enabling
/// </summary>
public sealed class Int64JsonConverter : CryptoIdJsonConverterBase<long>
{
    /// <summary>
    /// Initializes a new instance of the Int64JsonConverter class.
    /// </summary>
    public Int64JsonConverter()
    {
    }

    /// <summary>
    /// Initializes a new instance of the Int32JsonConverter class
    /// </summary>
    /// <param name="registryKey">registry key of encoder</param>
    public Int64JsonConverter(string registryKey): base(registryKey)
    {
    }

    /// <summary>
    /// Reads and converts the JSON representation of a long value to its corresponding Int64CryptoId representation.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!IsKeyed)
        {
            return CryptoIdRegistry.Int64Encoder.Decode(reader.GetString());
        }
        return CryptoIdRegistry.Get<long>(Key).Decode(reader.GetString());
    }

    /// <summary>
    /// Writes a long value as its corresponding Int64CryptoId representation in JSON format.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        if (!IsKeyed)
        {
            writer.WriteStringValue(CryptoIdRegistry.Int64Encoder.Encode(value));
            return;
        }
        writer.WriteStringValue(CryptoIdRegistry.Get<long>(Key!).Encode(value));
    }
}
