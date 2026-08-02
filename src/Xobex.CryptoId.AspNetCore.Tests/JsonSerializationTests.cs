// <copyright file="JsonSerializationTests.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Net;
using System.Text;
using System.Text.Json;

namespace Xobex.CryptoId.AspNetCore.Tests;

[TestClass]
public class JsonSerializationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions DeserializeOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record PayloadDto(string? LongId, string? IntId, string? KeyedLongId);

    [TestMethod]
    public async Task JsonGet_SerializesWithDefaultAndKeyedEncoders()
    {
        using var response = await Client.GetAsync("/api/images/payload");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var dto = JsonSerializer.Deserialize<PayloadDto>(await response.Content.ReadAsStringAsync(), DeserializeOptions);
        Assert.IsNotNull(dto);

        Assert.AreEqual(1234567890123L, Int64Encoder.Decode(dto!.LongId));
        Assert.AreEqual(42, Int32Encoder.Decode(dto.IntId));
        Assert.AreEqual(7L, KeyedEncoder.Decode(dto.KeyedLongId));
    }

    [TestMethod]
    public async Task JsonPost_RoundTrip()
    {
        using var get = await Client.GetAsync("/api/images/payload");
        var json = await get.Content.ReadAsStringAsync();

        using var post = await Client.PostAsync("/api/images/payload", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.AreEqual(HttpStatusCode.OK, post.StatusCode);
        var dto = JsonSerializer.Deserialize<PayloadDto>(await post.Content.ReadAsStringAsync(), DeserializeOptions);
        Assert.IsNotNull(dto);

        Assert.AreEqual(1234567890123L, Int64Encoder.Decode(dto!.LongId));
        Assert.AreEqual(42, Int32Encoder.Decode(dto.IntId));
        Assert.AreEqual(7L, KeyedEncoder.Decode(dto.KeyedLongId));
    }

    [TestMethod]
    public async Task JsonPost_InvalidToken_ReturnsBadRequest()
    {
        using var response = await Client.PostAsync(
            "/api/images/payload",
            new StringContent("{\"longId\":\"!!!bad-token!!!\"}", Encoding.UTF8, "application/json"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task JsonPost_NullLongId_ReturnsBadRequest()
    {
        using var response = await Client.PostAsync(
            "/api/images/payload",
            new StringContent("{\"longId\":null}", Encoding.UTF8, "application/json"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
