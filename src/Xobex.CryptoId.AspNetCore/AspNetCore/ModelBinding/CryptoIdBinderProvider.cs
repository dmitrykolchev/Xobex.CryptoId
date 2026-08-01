// <copyright file="CryptoIdBinderProvider.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Xobex.CryptoId.AspNetCore.ModelBinding;

/// <summary>
/// Provides model binders for the CryptoId types (<see cref="Int32CryptoId"/> and <see cref="Int64CryptoId"/>).
/// </summary>
public sealed class CryptoIdBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Gets the appropriate model binder for the specified model type.
    /// </summary>
    /// <param name="context">The model binder provider context.</param>
    /// <returns>
    /// The model binder for <see cref="Int32CryptoId"/> or <see cref="Int64CryptoId"/>,
    /// or null if the model type is not a CryptoId type.
    /// </returns>
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
