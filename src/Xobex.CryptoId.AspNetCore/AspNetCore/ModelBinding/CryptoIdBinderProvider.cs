// <copyright file="CryptoIdBinderProvider.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// Provides a model binder for CryptoId types (Int32CryptoId and Int64CryptoId) and their corresponding primitive types (int and long).
/// </summary>
public class CryptoIdBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Gets the appropriate model binder for the specified model type.
    /// </summary>
    /// <param name="context">The model binder provider context.</param>
    /// <returns></returns>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(Int32CryptoId))
        {
            var encoder = context.Services.GetRequiredService<ICryptoIdEncoder<int>>();
            return new Int32CryptoIdBinder(encoder);
        }
        else if (context.Metadata.ModelType == typeof(Int64CryptoId))
        {
            var encoder = context.Services.GetRequiredService<ICryptoIdEncoder<long>>();
            return new Int64CryptoIdBinder(encoder);
        }
        return null;
    }
}
