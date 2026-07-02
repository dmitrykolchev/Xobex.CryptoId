// <copyright file="Int64CryptoIdConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

internal class Int64CryptoIdConverter : JsonConverter<Int64CryptoId>
{
    internal static ICryptoIdEncoder<long>? _encoder;

    public Int64CryptoIdConverter()
    {
    }

    public override Int64CryptoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Int64CryptoId)_encoder!.Decode(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, Int64CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder!.Encode(value.Value));
    }
}
