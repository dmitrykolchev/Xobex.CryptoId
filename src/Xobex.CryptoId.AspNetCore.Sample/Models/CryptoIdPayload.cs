// <copyright file="CryptoIdPayload.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.CryptoId.Json.Serialization;

namespace Xobex.CryptoId.AspNetCore.Sample.Models;

public class CryptoIdPayload
{
    public Int64CryptoId LongId { get; set; }

    public Int32CryptoId IntId { get; set; }

    [CryptoIdJsonConverter("DetAes")]
    public Int64CryptoId KeyedLongId { get; set; }
}
