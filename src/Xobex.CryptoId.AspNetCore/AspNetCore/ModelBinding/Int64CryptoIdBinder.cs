// <copyright file="Int64CryptoIdBinder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// A model binder for the Int64CryptoId type, which decodes a string representation of a CryptoId into
/// </summary>
public sealed class Int64CryptoIdBinder : IModelBinder
{
    /// <summary>
    /// Gets the singleton instance of the Int64CryptoIdBinder.
    /// </summary>
    public static readonly Int64CryptoIdBinder Instance = new();

    private Int64CryptoIdBinder() { }
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
            bindingContext.Result = ModelBindingResult.Success(new Int64CryptoId(CryptoIdRegistry.Int64Encoder.Decode(value)));
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// A model binder for the long type, which decodes a string representation of a CryptoId into
/// </summary>
public sealed class Int64Binder : IModelBinder
{
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
            bindingContext.Result = ModelBindingResult.Success(CryptoIdRegistry.Int64Encoder.Decode(value));
        }
        return Task.CompletedTask;
    }
}
