// <copyright file="Int32JsonConverter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text.Json;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Json.Serialization;

/// <summary>
/// Represents a JSON converter for the Int32CryptoId type, enabling
/// custom serialization and deserialization of cryptographically
/// encoded integer identifiers.
/// </summary>
public sealed class Int32JsonConverter : CryptoIdJsonConverterBase<int>
{
    private readonly ICryptoIdEncoder<int> _encoder;

    /// <summary>
    /// Initializes a new instance of the Int32JsonConverter class.
    /// </summary>
    public Int32JsonConverter()
    {
        _encoder = CryptoIdRegistry.Int32Encoder ?? throw new InvalidOperationException("encoder not registered");
    }

    /// <summary>
    /// Initializes a new instance of the Int32JsonConverter class
    /// </summary>
    /// <param name="registryKey">registry key of encoder</param>
    public Int32JsonConverter(string registryKey) : base(registryKey)
    {
        _encoder = (ICryptoIdEncoder<int>)CryptoIdRegistry.Get(registryKey);
    }

    /// <summary>
    /// Reads and converts the JSON representation of an Int32CryptoId to its integer value.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return _encoder.Decode(reader.GetString());
    }

    /// <summary>
    /// Writes the integer value of an Int32CryptoId to its JSON representation.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_encoder.Encode(value));
    }
}
