// <copyright file="Int32CryptoId.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId;

/// <summary>
/// Represents a cryptographically encoded identifier for an integer value, providing
/// type safety and custom serialization/deserialization behavior.
/// </summary>
[JsonConverter(typeof(Int32CryptoIdConverter))]
public readonly struct Int32CryptoId : IEquatable<Int32CryptoId>
{
    /// <summary>
    /// Represents an empty or uninitialized Int32CryptoId with a value of 0.
    /// </summary>
    public static readonly Int32CryptoId Zero = new(0);

    private readonly int _value;

    /// <summary>
    /// Initializes a new instance of the Int32CryptoId struct with a default value of 0.
    /// </summary>
    public Int32CryptoId() : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Int32CryptoId struct with the specified integer value.
    /// </summary>
    /// <param name="value"></param>
    public Int32CryptoId(int value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets a value indicating whether the Int32CryptoId is empty (i.e., has a value of 0).
    /// </summary>
    public bool IsEmpty => _value == 0;

    /// <summary>
    /// Gets the underlying integer value of the Int32CryptoId.
    /// </summary>
    public int Value => _value;

    /// <summary>
    /// Defines an explicit conversion from Int32CryptoId to int, allowing the extraction of the underlying integer value.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator int(Int32CryptoId value)
    {
        return value._value;
    }

    /// <summary>
    /// Defines an explicit conversion from int to Int32CryptoId, allowing the creation of an Int32CryptoId from an integer value.
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator Int32CryptoId(int value)
    {
        return new Int32CryptoId(value);
    }

    /// <summary>
    /// Determines whether the current Int32CryptoId is equal to another Int32CryptoId instance.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Int32CryptoId other)
    {
        return _value == other._value;
    }

    /// <summary>
    /// Determines whether the current Int32CryptoId is equal to another object, which must be an Int32CryptoId for equality to be true.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is Int32CryptoId other)
        {
            return Equals(other);
        }
        return false;
    }

    /// <summary>
    /// Returns a hash code for the current Int32CryptoId, based on its underlying integer value.
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

    /// <summary>
    /// Defines the equality operator for Int32CryptoId, allowing comparison
    /// of two instances for equality based on their underlying integer values.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Int32CryptoId left, Int32CryptoId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Defines the inequality operator for Int32CryptoId, allowing comparison
    /// of two instances for inequality based on their underlying integer values.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Int32CryptoId left, Int32CryptoId right)
    {
        return !left.Equals(right);
    }
}
