// <copyright file="ICryptoIdEncoder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Cryptography.Abstractions;

/// <summary>
/// Defines the contract for encrypting and decrypting cryptographic identifiers.
/// </summary>
public interface ICryptoIdEncoder
{
    /// <summary>
    /// Encrypts an identifier and encodes it to a URL-safe Base64 string.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    string Encode(object id);

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <returns>The decrypted identifier.</returns>
    object Decode(ReadOnlySpan<char> urlEncodedBase64);

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <param name="value">The decrypted identifier</param>
    /// <returns>Returns true is decryption was successfull</returns>
    bool TryDecode(ReadOnlySpan<char> urlEncodedBase64, out object value);

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
    bool TryEncode(object id, Span<char> destination, out int charsWritten);

    /// <summary>
    /// Gets the type of the identifier that this encoder can process.
    /// </summary>
    Type IdType { get; }

    /// <summary>
    /// Gets the size of the identifier in bytes.
    /// </summary>
    int IdSizeInBytes { get; }
}
