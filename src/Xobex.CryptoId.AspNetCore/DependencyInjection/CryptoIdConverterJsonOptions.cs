// <copyright file="CryptoIdConverterOptions.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.DependencyInjection;

/// <summary>
/// 
/// </summary>
public class CryptoIdConverterJsonOptions : IConfigureOptions<JsonOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of <see cref="CryptoIdConverterJsonOptions"/> class.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public CryptoIdConverterJsonOptions(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Configure(JsonOptions options)
    {
        //options.JsonSerializerOptions.Converters.Add(
        //            new Int32CryptoIdConverter(_httpContextAccessor));
    }
}
