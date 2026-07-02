// <copyright file="Int32CryptoIdBinder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// A model binder for the Int32CryptoId type, which decodes a string representation of a CryptoId into
/// an Int32CryptoId instance using the provided ICryptoIdEncoder&lt;int&gt;.
/// </summary>
public class Int32CryptoIdBinder : IModelBinder
{
    private readonly ICryptoIdEncoder<int> _encoder;

    /// <summary>
    /// Initializes a new instance of the Int32CryptoIdBinder class with the specified ICryptoIdEncoder&lt;int&gt;.
    /// </summary>
    /// <param name="encoder"></param>
    public Int32CryptoIdBinder(ICryptoIdEncoder<int> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _encoder = encoder;
    }

    /// <summary>
    /// Binds the model by decoding the string representation of a CryptoId into an Int32CryptoId instance.
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (!string.IsNullOrEmpty(value))
        {
            bindingContext.Result = ModelBindingResult.Success(new Int32CryptoId(_encoder.Decode(value)));
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// A model binder for the int type, which decodes a string representation of a CryptoId into
/// </summary>
public class Int32Binder : IModelBinder
{
    private readonly ICryptoIdEncoder<int> _encoder;

    /// <summary>
    /// Initializes a new instance of the Int32Binder class with the specified ICryptoIdEncoder&lt;int&gt;.
    /// </summary>
    /// <param name="encoder"></param>
    public Int32Binder(ICryptoIdEncoder<int> encoder)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        _encoder = encoder;
    }

    /// <summary>
    /// Binds the model by decoding the string representation of a CryptoId into an int value.
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
