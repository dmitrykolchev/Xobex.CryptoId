// <copyright file="Int32CryptoIdBinder.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.Cryptography.Abstractions;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// A model binder for the <see cref="Int32CryptoId"/> type, which decodes a CryptoId string
/// into an <see cref="Int32CryptoId"/> instance using the registered <see cref="ICryptoIdEncoder{T}"/>.
/// </summary>
public sealed class Int32CryptoIdBinder : IModelBinder
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="Int32CryptoIdBinder"/>.
    /// </summary>
    public static readonly Int32CryptoIdBinder Instance = new();

    private Int32CryptoIdBinder() { }

    /// <summary>
    /// Binds the model by decoding the string representation of a CryptoId into an <see cref="Int32CryptoId"/> instance.
    /// </summary>
    /// <param name="bindingContext">The model binding context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
/// A model binder for the <see cref="int"/> type, which decodes a CryptoId string into an integer value.
/// </summary>
public sealed class Int32Binder : IModelBinder
{
    /// <summary>
    /// Binds the model by decoding the string representation of a CryptoId into an integer value.
    /// </summary>
    /// <param name="bindingContext">The model binding context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (value == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var valueString = value.FirstValue;
        if (string.IsNullOrEmpty(valueString) || !CryptoIdRegistry.Int32Encoder.TryDecode(valueString, out var decoded))
        {
            bindingContext.Result = ModelBindingResult.Success(CryptoIdRegistry.Int32Encoder.Decode(value));
        }
        return Task.CompletedTask;
    }
}
