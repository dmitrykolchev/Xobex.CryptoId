// <copyright file="ApiController.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Xobex.CryptoId.AspNetCore.Sample.Models;

namespace Xobex.CryptoId.AspNetCore.Sample.Controllers;

[ApiController]
[Route("api/images")]
public class ApiController : ControllerBase
{
    [HttpGet("payload")]
    public IActionResult GetPayload()
    {
        return Ok(new CryptoIdPayload { LongId = (Int64CryptoId)1234567890123L, IntId = (Int32CryptoId)42, KeyedLongId = (Int64CryptoId)7L });
    }

    [HttpPost("payload")]
    public IActionResult EchoPayload([FromBody] CryptoIdPayload model)
    {
        return Ok(model);
    }
}
