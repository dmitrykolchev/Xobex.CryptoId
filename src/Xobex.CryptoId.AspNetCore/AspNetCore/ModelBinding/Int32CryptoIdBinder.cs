// <copyright file="Int32CryptoIdBinder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// A model binder for the Int32CryptoId type, which decodes a string representation of a CryptoId into
/// an Int32CryptoId instance using the provided ICryptoIdEncoder&lt;int&gt;.
/// </summary>
public sealed class Int32CryptoIdBinder : IModelBinder
{
    /// <summary>
    /// Gets the singleton instance of the Int32CryptoIdBinder.
    /// </summary>
    public static readonly Int32CryptoIdBinder Instance = new();

    private Int32CryptoIdBinder() { }

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
            bindingContext.Result = ModelBindingResult.Success(new Int32CryptoId(CryptoIdRegistry.Int32Encoder.Decode(value)));
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// A model binder for the int type, which decodes a string representation of a CryptoId into
/// </summary>
public class Int32Binder : IModelBinder
{
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
            bindingContext.Result = ModelBindingResult.Success(CryptoIdRegistry.Int32Encoder.Decode(value));
        }
        return Task.CompletedTask;
    }
}
