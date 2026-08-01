// <copyright file="Int64CryptoIdConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the <see cref="Int64CryptoId"/> type, enabling
/// custom serialization and deserialization of cryptographically encoded integer identifiers.
/// </summary>
public sealed class Int64CryptoIdConverter : CryptoIdJsonConverterBase<Int64CryptoId>
{
    private readonly ICryptoIdEncoder<long> _encoder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Int64CryptoIdConverter"/> class using
    /// the default encoder registered for <see cref="long"/> identifiers.
    /// </summary>
    public Int64CryptoIdConverter()
    {
        _encoder = CryptoIdRegistry.Int64Encoder ?? throw new InvalidOperationException("encoder not registered");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Int64CryptoIdConverter"/> class using
    /// the encoder registered under the specified registry key.
    /// </summary>
    /// <param name="registryKey">The registry key of the encoder to use.</param>
    public Int64CryptoIdConverter(string registryKey) : base(registryKey)
    {
        _encoder = (ICryptoIdEncoder<long>)CryptoIdRegistry.Get(registryKey);
    }

    /// <summary>
    /// Reads a JSON string and decodes it to an <see cref="Int64CryptoId"/>.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The decoded <see cref="Int64CryptoId"/>.</returns>
    public override Int64CryptoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Int64CryptoId)_encoder.Decode(reader.GetString());
    }

    /// <summary>
    /// Writes an <see cref="Int64CryptoId"/> as an encoded JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Int64CryptoId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder.Encode(value.Value));
    }
}
