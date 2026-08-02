// <copyright file="CryptoIdServiceCollectionExtensions.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    /// Adds CryptoId services to the specified <see cref="IServiceCollection"/>,
    /// configuring <see cref="CryptoIdOptions"/> from the specified configuration section.
    /// </summary>
    /// <param name="services">The service collection to add the CryptoId services to.</param>
    /// <param name="section">The configuration section containing the CryptoId options
    /// (<c>Secret</c>, <c>Salt</c>, <c>Int32Algorithm</c>, <c>Int64Algorithm</c>).</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="section"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CryptoIdOptions.Salt"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="CryptoIdOptions.Salt"/> is not a valid hexadecimal string or is shorter than 8 bytes,
    /// or when <see cref="CryptoIdOptions.Secret"/> is null or empty.
    /// </exception>
    public static IServiceCollection AddCryptoId(this IServiceCollection services, IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        var options = new CryptoIdOptions();
        section.Bind(options);
        return services.AddCryptoId(options);
    }

    /// <summary>
    /// Adds CryptoId services to the specified <see cref="IServiceCollection"/>,
    /// configuring <see cref="CryptoIdOptions"/> with the provided delegate.
    /// </summary>
    /// <param name="services">The service collection to add the CryptoId services to.</param>
    /// <param name="configure">The delegate used to configure the <see cref="CryptoIdOptions"/>.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CryptoIdOptions.Salt"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="CryptoIdOptions.Salt"/> is not a valid hexadecimal string or is shorter than 8 bytes,
    /// or when <see cref="CryptoIdOptions.Secret"/> is null or empty.
    /// </exception>
    public static IServiceCollection AddCryptoId(this IServiceCollection services, Action<CryptoIdOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var options = new CryptoIdOptions();
        configure(options);
        return services.AddCryptoId(options);
    }

    /// <summary>
    /// Adds CryptoId services to the specified <see cref="IServiceCollection"/> with the provided options.
    /// </summary>
    /// <param name="services">The service collection to add the CryptoId services to.</param>
    /// <param name="options">The options that configure the cipher algorithms, secret and salt.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CryptoIdOptions.Salt"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="CryptoIdOptions.Salt"/> is not a valid hexadecimal string or is shorter than 8 bytes,
    /// or when <see cref="CryptoIdOptions.Secret"/> is null or empty.
    /// </exception>
    public static IServiceCollection AddCryptoId(this IServiceCollection services, CryptoIdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Salt))
        {
            throw new InvalidOperationException(
                "A stable salt must be configured via CryptoIdOptions.Salt as a hexadecimal string " +
                "(e.g., loaded from configuration or a secret store). A per-process random salt would make " +
                "previously issued IDs undecodable after an application restart.");
        }

        byte[] salt;
        try
        {
            salt = Convert.FromHexString(options.Salt);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(
                "CryptoIdOptions.Salt must be a valid hexadecimal string.",
                nameof(options),
                ex);
        }

        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes.", nameof(options));
        }

        if (string.IsNullOrEmpty(options.Secret))
        {
            throw new ArgumentException(
                "A stable secret must be configured via CryptoIdOptions.Secret " +
                "(e.g., loaded from configuration or a secret store). A per-process random " +
                "secret would make previously issued IDs undecodable after an application restart.",
                nameof(options));
        }

        CryptoIdRegistry.Register(CryptoIdFactory.Create<int>(options.Int32Algorithm, options.Secret, salt));
        CryptoIdRegistry.Register(CryptoIdFactory.Create<long>(options.Int64Algorithm, options.Secret, salt));

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
    /// Registers a keyed encoder for an additional cipher configuration, identified by a service key.
    /// </summary>
    /// <param name="services">The service collection to add the encoder to.</param>
    /// <param name="serviceKey">The unique key used to retrieve the encoder.</param>
    /// <param name="algorithm">The cipher algorithm to use for encoding and decoding.</param>
    /// <param name="secret">The secret key material used for encoding and decoding.</param>
    /// <param name="salt">The salt for HKDF key derivation, stable across restarts.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceKey"/>, <paramref name="secret"/>, or <paramref name="salt"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the <paramref name="serviceKey"/> is already registered.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="salt"/> is shorter than 8 bytes.</exception>
    public static IServiceCollection AddKeyedEncoder(this IServiceCollection services, string serviceKey, IdCipherAlgorithm algorithm, string secret, byte[] salt)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(serviceKey);
        ArgumentNullException.ThrowIfNullOrEmpty(secret);
        ArgumentNullException.ThrowIfNull(salt);

        if (salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes.", nameof(salt));
        }
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
