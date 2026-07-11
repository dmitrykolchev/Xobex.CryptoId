// <copyright file="ImageModel.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.AspNetCore.Sample.Models;

public class ImageModel
{
    [CryptoIdJsonConverter("DetAes")]
    public Int64CryptoId Id { get; set; }

    public string? Name { get; set; }
}
