// <copyright file="Int32JsonConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xobex.CryptoId.Json.Serialization;

internal class Int32JsonConverter : JsonConverter<int>
{
    public Int32JsonConverter()
    {
    }

    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Int32CryptoIdConverter._encoder!.Decode(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Int32CryptoIdConverter._encoder!.Encode(value));
    }
}
