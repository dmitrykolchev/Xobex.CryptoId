// <copyright file="IntegrationTestBase.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xobex.Cryptography.Abstractions;
using Xobex.CryptoId.AspNetCore.Sample;

namespace Xobex.CryptoId.AspNetCore.Tests;

/// <summary>
/// Base class for integration tests that use a single shared instance of the sample
/// application. The host is built exactly once per test process because the underlying
/// <see cref="CryptoIdRegistry"/> is a process-wide static registry and re-running the
/// sample's Program would re-register keyed encoders and throw on duplicate keys.
/// </summary>
public abstract class IntegrationTestBase
{
    private static readonly WebApplicationFactory<Program> AppFactory = BuildFactory();

    private static WebApplicationFactory<Program> BuildFactory()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("https_port", string.Empty));
        // Force the host to build once so Program.Main runs exactly once.
        _ = factory.Server;
        return factory;
    }

    /// <summary>
    /// Gets an <see cref="HttpClient"/> configured to send requests to the test server.
    /// </summary>
    protected static HttpClient Client => AppFactory.CreateClient();

    /// <summary>
    /// Gets the application's service provider.
    /// </summary>
    protected static IServiceProvider AppServices => AppFactory.Services;

    /// <summary>
    /// Gets the default <see cref="int"/> encoder resolved from the application's DI container.
    /// </summary>
    protected static ICryptoIdEncoder<int> Int32Encoder =>
        AppFactory.Services.GetRequiredService<ICryptoIdEncoder<int>>();

    /// <summary>
    /// Gets the default <see cref="long"/> encoder resolved from the application's DI container.
    /// </summary>
    protected static ICryptoIdEncoder<long> Int64Encoder =>
        AppFactory.Services.GetRequiredService<ICryptoIdEncoder<long>>();

    /// <summary>
    /// Gets the keyed ("DetAes") <see cref="long"/> encoder registered via AddKeyedEncoder.
    /// </summary>
    protected static ICryptoIdEncoder<long> KeyedEncoder =>
        AppFactory.Services.GetRequiredKeyedService<ICryptoIdEncoder<long>>("DetAes");
}
