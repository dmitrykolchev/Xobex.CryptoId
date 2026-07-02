// <copyright file="CryptoIdBinderProvider.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            return Int32CryptoIdBinder.Instance;
        }
        else if (context.Metadata.ModelType == typeof(Int64CryptoId))
        {
            return Int64CryptoIdBinder.Instance;
        }
        return null;
    }
}
