// <copyright file="OperationResult.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Security.Cryptography;

namespace Xobex.Cryptography;

/// <summary>
/// 
/// </summary>
public enum OperationResultType
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
    /// 
    /// </summary>
    FormatError,
    /// <summary>
    /// 
    /// </summary>
    CryptographicError,
    /// <summary>
    /// 
    /// </summary>
    DisposedError,
}

/// <summary>
/// 
/// </summary>
public readonly ref struct OperationResult
{
    /// <summary>
    /// 
    /// </summary>
    public static OperationResult Success => new(true);

    /// <summary>
    /// 
    /// </summary>
    public static OperationResult Fail => new(false);

    /// <summary>
    /// Initializes new instance of <see cref="OperationResult"/> 
    /// </summary>
    /// <param name="succeeded"></param>
    public OperationResult(bool succeeded)
    {
        Result = succeeded ? OperationResultType.Succeeded : OperationResultType.Failed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resultType"></param>
    /// <param name="message"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public OperationResult(OperationResultType resultType, string? message = null)
    {
        Result = resultType;
        Message = message;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool Succeeded => Result == OperationResultType.Succeeded;

    /// <summary>
    /// 
    /// </summary>
    public bool Failed => Result != OperationResultType.Succeeded;

    /// <summary>
    /// Gets the operation result type
    /// </summary>
    public OperationResultType Result { get; }

    /// <summary>
    /// Gets a result message
    /// </summary>
    public string? Message => field ?? (Result != OperationResultType.Succeeded ? "Operation Failed" : null);

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void ThrowIfFailed()
    {
        Exception? ex = Result switch
        {
            OperationResultType.Succeeded => null,
            OperationResultType.FormatError => new FormatException(Message),
            OperationResultType.CryptographicError => new CryptographicException(Message),
            OperationResultType.Failed => new InvalidOperationException(Message),
            OperationResultType.DisposedError => new ObjectDisposedException(Message),
            _ => throw new NotImplementedException(),
        };
        if (ex != null)
        {
            throw ex;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator bool(OperationResult result)
    {
        return result.Result == OperationResultType.Succeeded;
    }
}
