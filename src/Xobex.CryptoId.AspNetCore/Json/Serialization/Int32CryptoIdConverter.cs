// <copyright file="" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using Xobex.Cryptography.Abstractions;


namespace Xobex.CryptoId.Json.Serialization;

internal class Int32CryptoIdConverter : JsonConverter<Int32CryptoId>
{
    internal static ICryptoIdEncoder<int>? _encoder;

    public Int32CryptoIdConverter()
    {
    }

    public override Int32CryptoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Int32CryptoId)_encoder!.Decode(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, Int32CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder!.Encode(value.Value));
    }
}
