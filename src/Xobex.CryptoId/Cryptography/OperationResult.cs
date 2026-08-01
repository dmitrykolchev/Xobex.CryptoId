// <copyright file="OperationResult.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics;
using System.Security.Cryptography;

namespace Xobex.Cryptography;

/// <summary>
/// Represents the result kind of a cryptographic operation.
/// </summary>
internal enum OperationResultKind
{
    /// <summary>
    /// Operation succeeded
    /// </summary>
    Succeeded = 0,
    /// <summary>
    /// Operation failed
    /// </summary>
    Failed = 1,
    /// <summary>
    /// Format error
    /// </summary>
    FormatError,
    /// <summary>
    /// Cryptographic error
    /// </summary>
    CryptographicError,
    /// <summary>
    /// Object disposed error
    /// </summary>
    DisposedError,
}

/// <summary>
/// Represents the result of a cryptographic operation.
/// </summary>
internal readonly struct OperationResult
{
    /// <summary>
    /// Gets the success instance of <see cref="OperationResult"/>.
    /// </summary>
    public static readonly OperationResult Success = new(OperationResultKind.Succeeded);

    /// <summary>
    /// Returns a failed operation result.
    /// </summary>
    /// <param name="kind">The failed operation result kind.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed <see cref="OperationResult"/>.</returns>
    public static OperationResult Fail(OperationResultKind kind, string message)
    {
        return new OperationResult(kind, message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult"/> struct.
    /// </summary>
    /// <param name="resultType">The kind of the operation result.</param>
    /// <param name="message">An optional error message.</param>
    public OperationResult(OperationResultKind resultType, string? message = null)
    {
        Kind = resultType;
        Message = message;
    }

    /// <summary>
    /// Returns true if the operation succeeded.
    /// </summary>
    public bool Succeeded => Kind == OperationResultKind.Succeeded;

    /// <summary>
    /// Returns true if the operation failed.
    /// </summary>
    public bool Failed => Kind != OperationResultKind.Succeeded;

    /// <summary>
    /// Gets the operation result type
    /// </summary>
    public OperationResultKind Kind { get; }

    /// <summary>
    /// Gets a result message
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Throws an exception if the operation did not succeed.
    /// </summary>
    public void ThrowIfFailed()
    {
        Exception? ex = Kind switch
        {
            OperationResultKind.Succeeded => null,
            OperationResultKind.FormatError => new FormatException(message: Message),
            OperationResultKind.CryptographicError => new CryptographicException(message: Message),
            OperationResultKind.Failed => new InvalidOperationException(message: Message),
            OperationResultKind.DisposedError => new ObjectDisposedException(objectName: null, message: Message),
            _ => throw new UnreachableException(),
        };
        if (ex != null)
        {
            throw ex;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Failed ? $"{Kind}: {Message ?? "No additional details provided"}" : nameof(OperationResultKind.Succeeded);
    }
}
