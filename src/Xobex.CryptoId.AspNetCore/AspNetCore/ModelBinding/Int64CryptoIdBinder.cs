// <copyright file="Int32CryptoIdBinder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// A model binder for the Int64CryptoId type, which decodes a string representation of a CryptoId into
/// </summary>
public class Int64CryptoIdBinder : IModelBinder
{
    private readonly ICryptoIdEncoder<long> _encoder;

    /// <summary>
    /// Initializes a new instance of the Int64CryptoIdBinder class with the specified ICryptoIdEncoder&lt;long&gt;.
    /// </summary>
    /// <param name="encoder"></param>
    public Int64CryptoIdBinder(ICryptoIdEncoder<long> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _encoder = encoder;
    }

    /// <summary>
    /// Binds the model by decoding the string representation of a CryptoId into an Int64CryptoId instance.
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (!string.IsNullOrEmpty(value))
        {
            bindingContext.Result = ModelBindingResult.Success(new Int64CryptoId(_encoder.Decode(value)));
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// A model binder for the long type, which decodes a string representation of a CryptoId into
/// </summary>
public class Int64Binder : IModelBinder
{
    private readonly ICryptoIdEncoder<long> _encoder;

    /// <summary>
    /// Initializes a new instance of the Int64Binder class with the specified ICryptoIdEncoder&lt;long&gt;.
    /// </summary>
    /// <param name="encoder"></param>
    public Int64Binder(ICryptoIdEncoder<long> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _encoder = encoder;
    }

    /// <summary>
    /// Binds the model by decoding the string representation of a CryptoId into a long value.
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (!string.IsNullOrEmpty(value))
        {
            bindingContext.Result = ModelBindingResult.Success(_encoder.Decode(value));
        }
        return Task.CompletedTask;
    }
}
