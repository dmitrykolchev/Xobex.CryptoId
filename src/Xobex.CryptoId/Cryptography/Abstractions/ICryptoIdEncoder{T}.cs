// <copyright file="ICryptoIdEncoder{T}.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Cryptography.Abstractions;

/// <summary>
/// Defines the contract for encrypting and decrypting cryptographic identifiers of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The data type of the identifier to be encoded/decoded. Must be a value type.</typeparam>
/// <remarks>
/// Implementations of this interface should provide encryption and decryption operations that convert
/// identifiers to and from URL-safe Base64 strings, suitable for use in web URLs and APIs.
/// </remarks>
public interface ICryptoIdEncoder<T> where T : struct
{
    /// <summary>
    /// Encrypts an identifier and encodes it to a URL-safe Base64 string.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    string Encode(T id);

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <returns>The decrypted identifier.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid URL-safe Base64 string or contains invalid data.</exception>
    T Decode(ReadOnlySpan<char> urlEncodedBase64);

    /// <summary>
    /// Gets a value indicating whether the encryption is deterministic (i.e., the same input always produces the same output).
    /// </summary>
    public bool IsDeterministic { get; }

    /// <summary>
    /// Attempts to encode an identifier into a provided character span, returning a boolean indicating success or failure.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="destination">The span of characters to write the encoded identifier to.</param>
    /// <param name="charsWritten">When the method returns, contains the number of characters written to the destination span.</param>
    /// <returns>true if the identifier was successfully encoded; otherwise, false.</returns>
    bool TryEncode(T id, Span<char> destination, out int charsWritten);
}
