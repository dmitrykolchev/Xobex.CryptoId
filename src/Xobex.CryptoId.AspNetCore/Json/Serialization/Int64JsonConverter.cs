// <copyright file="Int64JsonConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

internal class Int64JsonConverter : JsonConverter<long>
{
    public Int64JsonConverter()
    {
    }

    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Int64CryptoIdConverter._encoder!.Decode(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Int64CryptoIdConverter._encoder!.Encode(value));
    }
}
