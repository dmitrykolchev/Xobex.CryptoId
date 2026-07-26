// <copyright file="OperationResult.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics;
using System.Security.Cryptography;

namespace Xobex.Cryptography;

/// <summary>
/// OperationResultKind enum
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
/// OperationResult class
/// </summary>
internal readonly struct OperationResult
{
    /// <summary>
    /// Gets Success instance if <see cref="OperationResult"/>
    /// </summary>
    public static OperationResult Success => new(true);

    /// <summary>
    /// Initializes new instance of <see cref="OperationResult"/> 
    /// </summary>
    /// <param name="succeeded"></param>
    public OperationResult(bool succeeded)
    {
        Kind = succeeded ? OperationResultKind.Succeeded : OperationResultKind.Failed;
    }

    /// <summary>
    /// Initializes new instance of <see cref="OperationResult"/> 
    /// </summary>
    /// <param name="resultType"></param>
    /// <param name="message"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public OperationResult(OperationResultKind resultType, string? message = null)
    {
        Kind = resultType;
        Message = message;
    }

    /// <summary>
    /// Returns true is result of succeeded operation
    /// </summary>
    public bool Succeeded => Kind == OperationResultKind.Succeeded;

    /// <summary>
    /// Returns true is result of failed operation
    /// </summary>
    public bool Failed => Kind != OperationResultKind.Succeeded;

    /// <summary>
    /// Gets the operation result type
    /// </summary>
    public OperationResultKind Kind { get; }

    /// <summary>
    /// Gets a result message
    /// </summary>
    public string? Message => field ?? (Kind != OperationResultKind.Succeeded ? "Operation Failed" : null);

    /// <summary>
    /// Throws exception if Kind is not Succeeded
    /// </summary>
    public void ThrowIfFailed()
    {
        Exception? ex = Kind switch
        {
            OperationResultKind.Succeeded => null,
            OperationResultKind.FormatError => new FormatException(Message),
            OperationResultKind.CryptographicError => new CryptographicException(Message),
            OperationResultKind.Failed => new InvalidOperationException(Message),
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
        return Failed ? $"{Kind}: {Message}" : nameof(OperationResultKind.Succeeded);
    }
}
