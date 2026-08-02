// <copyright file="Int32CryptoIdConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the <see cref="Int32CryptoId"/> type, enabling
/// custom serialization and deserialization of cryptographically encoded integer identifiers.
/// </summary>
public sealed class Int32CryptoIdConverter : CryptoIdJsonConverterBase<Int32CryptoId>
{
    private readonly ICryptoIdEncoder<int> _encoder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Int32CryptoIdConverter"/> class using
    /// the default encoder registered for <see cref="int"/> identifiers.
    /// </summary>
    public Int32CryptoIdConverter()
    {
        _encoder = CryptoIdRegistry.Int32Encoder;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Int32CryptoIdConverter"/> class using
    /// the encoder registered under the specified registry key.
    /// </summary>
    /// <param name="registryKey">The registry key of the encoder to use.</param>
    public Int32CryptoIdConverter(string registryKey) : base(registryKey)
    {
        _encoder = (ICryptoIdEncoder<int>)CryptoIdRegistry.Get(registryKey);
    }

    /// <summary>
    /// Reads a JSON string and decodes it to an <see cref="Int32CryptoId"/>.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The decoded <see cref="Int32CryptoId"/>.</returns>
    public override Int32CryptoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Int32CryptoId)_encoder.Decode(reader.GetString());
    }

    /// <summary>
    /// Writes an <see cref="Int32CryptoId"/> as an encoded JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Int32CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder.Encode(value.Value));
    }
}
