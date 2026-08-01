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

    /// <summary>
    /// Initializes a new instance of the Int32CryptoId struct with a default value of 0.
    /// </summary>
    public Int32CryptoId() : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Int32CryptoId"/> struct with the specified integer value.
    /// </summary>
    /// <param name="value">The integer value of the CryptoId.</param>
    public Int32CryptoId(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets a value indicating whether the Int32CryptoId is empty (i.e., has a value of 0).
    /// </summary>
    public bool IsEmpty => Value == 0;

    /// <summary>
    /// Gets the underlying integer value of the Int32CryptoId.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Defines an explicit conversion from <see cref="Int32CryptoId"/> to <see cref="int"/>, allowing the extraction of the underlying integer value.
    /// </summary>
    /// <param name="value">The <see cref="Int32CryptoId"/> to convert.</param>
    public static explicit operator int(Int32CryptoId value)
    {
        return value.Value;
    }

    /// <summary>
    /// Defines an explicit conversion from <see cref="int"/> to <see cref="Int32CryptoId"/>, allowing the creation of an Int32CryptoId from an integer value.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static explicit operator Int32CryptoId(int value)
    {
        return new Int32CryptoId(value);
    }

    /// <summary>
    /// Determines whether the current <see cref="Int32CryptoId"/> is equal to another <see cref="Int32CryptoId"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="Int32CryptoId"/> to compare with this instance.</param>
    /// <returns>true if equal; otherwise, false.</returns>
    public bool Equals(Int32CryptoId other)
    {
        return Value == other.Value;
    }

    /// <summary>
    /// Determines whether the current <see cref="Int32CryptoId"/> is equal to another object, which must be an Int32CryptoId for equality to be true.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>true if equal; otherwise, false.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Int32CryptoId other)
        {
            return Equals(other);
        }
        return false;
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="Int32CryptoId"/>, based on its underlying integer value.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Int32CryptoId"/>.</returns>
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
    /// Defines the equality operator for <see cref="Int32CryptoId"/>, allowing comparison
    /// of two instances for equality based on their underlying integer values.
    /// </summary>
    /// <param name="left">The first <see cref="Int32CryptoId"/> to compare.</param>
    /// <param name="right">The second <see cref="Int32CryptoId"/> to compare.</param>
    /// <returns>true if the values are equal; otherwise, false.</returns>
    public static bool operator ==(Int32CryptoId left, Int32CryptoId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Defines the inequality operator for <see cref="Int32CryptoId"/>, allowing comparison
    /// of two instances for inequality based on their underlying integer values.
    /// </summary>
    /// <param name="left">The first <see cref="Int32CryptoId"/> to compare.</param>
    /// <param name="right">The second <see cref="Int32CryptoId"/> to compare.</param>
    /// <returns>true if the values are not equal; otherwise, false.</returns>
    public static bool operator !=(Int32CryptoId left, Int32CryptoId right)
    {
        return !left.Equals(right);
    }
}
