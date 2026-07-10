// <copyright file="CryptoIdServiceCollectionExtensions.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Security.Cryptography;
using Xobex.Cryptography;
using Xobex.Cryptography.Abstractions;
using Xobex.CryptoId.AspNetCore.ModelBinding;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.DependencyInjection;

/// <summary>
/// Provides extension methods for registering CryptoId services in an IServiceCollection, allowing
/// for the configuration of cipher algorithms and secret keys for encoding and decoding IDs.
/// </summary>
public static class CryptoIdServiceCollectionExtensions
{
    /// <summary>
    /// Adds CryptoId services to the specified IServiceCollection with default options.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCryptoId(this IServiceCollection services)
    {
        services.AddCryptoId(new CryptoIdOptions());
        return services;
    }

    /// <summary>
    /// Adds CryptoId services to the specified IServiceCollection with the provided options.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddCryptoId(this IServiceCollection services, CryptoIdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrEmpty(options.Secret))
        {
            var entropy = RandomNumberGenerator.GetBytes(64);
            options.Secret = Convert.ToBase64String(entropy);
        }
        CryptoIdRegistry.Register(CryptoIdFactory.Create<int>(options.Int32Algorithm, options.Secret, options.Salt));
        CryptoIdRegistry.Register(CryptoIdFactory.Create<long>(options.Int64Algorithm, options.Secret, options.Salt));

        services.AddSingleton<ICryptoIdEncoder<int>>(serviceProvider =>
        {
            return CryptoIdRegistry.Int32Encoder;
        });
        services.AddSingleton<ICryptoIdEncoder<long>>(serviceProvider =>
        {
            return CryptoIdRegistry.Int64Encoder;
        });
        services.AddMvcCore(options =>
        {
            options.ModelBinderProviders.Insert(0, new CryptoIdBinderProvider());
        });
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceKey"></param>
    /// <param name="algorithm"></param>
    /// <param name="secret"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddKeyedEncoder(this IServiceCollection services, string serviceKey, IdCipherAlgorithm algorithm, string secret, byte[]? salt)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(serviceKey);
        ArgumentNullException.ThrowIfNullOrEmpty(secret);
        if (algorithm is IdCipherAlgorithm.Skip32 or IdCipherAlgorithm.Speck32_64)
        {
            var encoder = CryptoIdFactory.Create<int>(algorithm, secret, salt);
            CryptoIdRegistry.Register(serviceKey, (ICryptoIdEncoder)encoder);
            services.AddKeyedSingleton(serviceKey, encoder);
        }
        else
        {
            var encoder = CryptoIdFactory.Create<long>(algorithm, secret, salt);
            CryptoIdRegistry.Register(serviceKey, (ICryptoIdEncoder)encoder);
            services.AddKeyedSingleton(serviceKey, encoder);
        }
        return services;
    }
}
