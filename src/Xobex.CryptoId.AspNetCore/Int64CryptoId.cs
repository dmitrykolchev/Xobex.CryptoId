// <copyright file="Int64CryptoId.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId;

/// <summary>
/// Represents a cryptographically encoded identifier for a long integer value, providing
/// </summary>
[JsonConverter(typeof(Int64CryptoIdConverter))]
public readonly struct Int64CryptoId: IEquatable<Int64CryptoId>
{
    /// <summary>
    /// Represents an empty or uninitialized Int64CryptoId with a value of 0.
    /// </summary>
    public static readonly Int64CryptoId Zero = new(0L);

    private readonly long _value;

    /// <summary>
    /// Initializes a new instance of the Int64CryptoId struct with the specified long integer value.
    /// </summary>
    /// <param name="value"></param>
    public Int64CryptoId(long value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the underlying long integer value of the Int64CryptoId.
    /// </summary>
    public long Value => _value;

    /// <summary>
    /// Defines an explicit conversion from Int64CryptoId to long, allowing the extraction of the underlying long integer value.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator long(Int64CryptoId value)
    {
        return value._value;
    }

    /// <summary>
    /// Defines an explicit conversion from long to Int64CryptoId, allowing the creation of an Int64CryptoId from a long integer value.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator Int64CryptoId(long value)
    {
        return new Int64CryptoId(value);
    }

    /// <summary>
    /// Determines whether the current Int64CryptoId is equal to another Int64CryptoId instance.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Int64CryptoId other)
    {
        return _value == other._value;
    }

    /// <summary>
    /// Determines whether the current Int64CryptoId is equal to another object, which can be an Int64CryptoId or any other type.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Int64CryptoId other)
        {
            return Equals(other);
        }
        return false;
    }

    /// <summary>
    /// Returns a hash code for the current Int64CryptoId, based on its underlying long integer value.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }
}
