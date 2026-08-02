// <copyright file="CryptoIdOptionsTests.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xobex.Cryptography.Abstractions;
using Xobex.CryptoId.AspNetCore.ModelBinding;
using Xobex.CryptoId.DependencyInjection;

namespace Xobex.CryptoId.AspNetCore.Tests;

[TestClass]
public class CryptoIdOptionsTests
{
    private const string Secret = "my-secret-key";
    private const string Salt = "2b7e151628aed2a6abf7158809cf4f3c";

    [TestMethod]
    public void AddCryptoId_WithoutSalt_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var options = new CryptoIdOptions { Secret = Secret };

        Assert.ThrowsExactly<InvalidOperationException>(() => services.AddCryptoId(options));
    }

    [TestMethod]
    public void AddCryptoId_WithNonHexSalt_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        var options = new CryptoIdOptions { Secret = Secret, Salt = "not-hex" };

        Assert.ThrowsExactly<ArgumentException>(() => services.AddCryptoId(options));
    }

    [TestMethod]
    public void AddCryptoId_WithShortSalt_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        var options = new CryptoIdOptions { Secret = Secret, Salt = "aabbccdd" };

        Assert.ThrowsExactly<ArgumentException>(() => services.AddCryptoId(options));
    }

    [TestMethod]
    public void AddCryptoId_WithoutSecret_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        var options = new CryptoIdOptions { Salt = Salt };

        Assert.ThrowsExactly<ArgumentException>(() => services.AddCryptoId(options));
    }

    [TestMethod]
    public void AddCryptoId_WithValidOptions_RegistersEncodersAndBinderProvider()
    {
        var services = new ServiceCollection();
        services.AddCryptoId(new CryptoIdOptions { Secret = Secret, Salt = Salt });

        var int32Descriptor = services.First(d => d.ServiceType == typeof(ICryptoIdEncoder<int>));
        Assert.IsNotNull(int32Descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, int32Descriptor.Lifetime);

        var int64Descriptor = services.First(d => d.ServiceType == typeof(ICryptoIdEncoder<long>));
        Assert.IsNotNull(int64Descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, int64Descriptor.Lifetime);

        using var provider = services.BuildServiceProvider();
        Assert.IsNotNull(provider.GetRequiredService<ICryptoIdEncoder<int>>());
        Assert.IsNotNull(provider.GetRequiredService<ICryptoIdEncoder<long>>());

        var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
        Assert.IsTrue(mvcOptions.ModelBinderProviders.Any(p => p is CryptoIdBinderProvider));
    }
}
