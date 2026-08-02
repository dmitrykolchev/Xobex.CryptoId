// <copyright file="DependencyInjectionTests.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Xobex.Cryptography.Abstractions;
using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.AspNetCore.Tests;

[TestClass]
public class DependencyInjectionTests : IntegrationTestBase
{
    [TestMethod]
    public void Int32Encoder_IsRegisteredAsSingleton()
    {
        using var scope1 = AppServices.CreateScope();
        using var scope2 = AppServices.CreateScope();

        var first = scope1.ServiceProvider.GetRequiredService<ICryptoIdEncoder<int>>();
        var second = scope2.ServiceProvider.GetRequiredService<ICryptoIdEncoder<int>>();

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void Int64Encoder_IsRegisteredAsSingleton()
    {
        using var scope1 = AppServices.CreateScope();
        using var scope2 = AppServices.CreateScope();

        var first = scope1.ServiceProvider.GetRequiredService<ICryptoIdEncoder<long>>();
        var second = scope2.ServiceProvider.GetRequiredService<ICryptoIdEncoder<long>>();

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void KeyedEncoder_IsRegisteredAndResolvable()
    {
        var keyed = AppServices.GetRequiredKeyedService<ICryptoIdEncoder<long>>("DetAes");
        var fromRegistry = CryptoIdRegistry.Get<long>("DetAes");

        Assert.IsNotNull(keyed);
        Assert.AreSame(fromRegistry, keyed);
    }

    [TestMethod]
    public void Registry_ExposesDefaultEncoders()
    {
        Assert.IsNotNull(CryptoIdRegistry.Int32Encoder);
        Assert.IsNotNull(CryptoIdRegistry.Int64Encoder);
    }
}
