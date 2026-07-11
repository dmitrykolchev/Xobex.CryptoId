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
public readonly struct Int64CryptoId : IEquatable<Int64CryptoId>
{
    /// <summary>
    /// Represents an empty or uninitialized Int64CryptoId with a value of 0.
    /// </summary>
    public static readonly Int64CryptoId Zero = new(0L);

    /// <summary>
    /// Initializes a new instance of the Int64CryptoId struct with a default value of 0.
    /// </summary>
    public Int64CryptoId() : this(0L)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Int64CryptoId struct with the specified long integer value.
    /// </summary>
    /// <param name="value"></param>
    public Int64CryptoId(long value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets a value indicating whether the Int32CryptoId is empty (i.e., has a value of 0).
    /// </summary>
    public bool IsEmpty => Value == 0;

    /// <summary>
    /// Gets the underlying long integer value of the Int64CryptoId.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Defines an explicit conversion from Int64CryptoId to long, allowing the extraction of the underlying long integer value.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator long(Int64CryptoId value)
    {
        return value.Value;
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
    /// Defines an explicit conversion from int to Int64CryptoId, allowing the creation of an Int64CryptoId from a int integer value.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator Int64CryptoId(int value)
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
        return Value == other.Value;
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
        return Value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Defines the equality operator for Int64CryptoId, allowing
    /// comparison of two Int64CryptoId instances for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Int64CryptoId left, Int64CryptoId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Defines the inequality operator for Int64CryptoId, allowing comparison
    /// of two instances for inequality based on their underlying integer values.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Int64CryptoId left, Int64CryptoId right)
    {
        return !left.Equals(right);
    }
}
