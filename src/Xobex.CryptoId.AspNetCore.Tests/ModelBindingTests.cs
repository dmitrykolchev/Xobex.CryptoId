// <copyright file="ModelBindingTests.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Net;

namespace Xobex.CryptoId.AspNetCore.Tests;

[TestClass]
public class ModelBindingTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Int32Binder_DecodesValidToken()
    {
        var token = Int32Encoder.Encode(42);
        using var response = await Client.GetAsync($"/GetItem1?id={token}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "int id = 42");
    }

    [TestMethod]
    public async Task Int32Binder_InvalidToken_ReturnsBadRequest()
    {
        using var response = await Client.GetAsync("/GetItem1?id=not-a-valid-token");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Int64Binder_DecodesValidToken()
    {
        var token = Int64Encoder.Encode(1234567890123L);
        using var response = await Client.GetAsync($"/GetItem4?id={token}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "long id = 1234567890123");
    }

    [TestMethod]
    public async Task Int64Binder_InvalidToken_ReturnsBadRequest()
    {
        using var response = await Client.GetAsync("/GetItem4?id=bad-token");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Int64CryptoId_DefaultBinder_DecodesValidToken()
    {
        var token = Int64Encoder.Encode(777L);
        using var response = await Client.GetAsync($"/GetItem5?id={token}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "Int64CryptoId id = 777");
    }

    [TestMethod]
    public async Task Int64CryptoId_DefaultBinder_InvalidToken_ReturnsBadRequest()
    {
        using var response = await Client.GetAsync("/GetItem5?id=bad-token");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Int32CryptoId_DefaultBinder_DecodesValidToken()
    {
        var token = Int32Encoder.Encode(123);
        using var response = await Client.GetAsync($"/GetItem2?id={token}&unused={token}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "Int32CryptoId id = 123");
    }

    [TestMethod]
    public async Task Int32CryptoId_DefaultBinder_InvalidToken_ReturnsBadRequest()
    {
        using var response = await Client.GetAsync("/GetItem2?id=bad-token&unused=bad-token");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TryDecodeEndpoint_ValidToken_ReturnsOk()
    {
        var token = Int64Encoder.Encode(99L);
        using var response = await Client.GetAsync($"/GetItem6?id={token}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TryDecodeEndpoint_InvalidToken_ReturnsBadRequest()
    {
        using var response = await Client.GetAsync("/GetItem6?id=bad-token");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
