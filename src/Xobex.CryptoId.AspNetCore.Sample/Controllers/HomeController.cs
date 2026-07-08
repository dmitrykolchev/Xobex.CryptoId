// <copyright file="HomeController.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xobex.Cryptography.Abstractions;
using Xobex.CryptoId.AspNetCore.ModelBinding;
using Xobex.CryptoId.AspNetCore.Sample.Models;

namespace Xobex.CryptoId.AspNetCore.Sample.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Implicit binding to specific DI encoder
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("GetItem1")]
    public IActionResult GetItem1([ModelBinder(typeof(Int32Binder))] int id)
    {
        return Ok($"int id = {id}");
    }

    /// <summary>
    /// Default parameter binding using DI encoders
    /// </summary>
    /// <param name="id"></param>
    /// <param name="unused"></param>
    /// <returns></returns>
    [HttpGet("GetItem2")]
    public IActionResult GetItem2([FromQuery] Int32CryptoId id, [FromQuery] Int32CryptoId unused)
    {
        if (!HttpContext.Request.Query.ContainsKey(nameof(id)))
        {
            Console.WriteLine($"Hasn't query paramer - {nameof(id)}");
        }
        if (!HttpContext.Request.Query.ContainsKey(nameof(unused)))
        {
            Console.WriteLine($"Hasn't query paramer - {nameof(unused)}");
        }
        var isIdBindingAttempted = ModelState.TryGetValue(nameof(id), out var _);
        var isUnusedBindingAttempted = ModelState.TryGetValue(nameof(unused), out var _);
        return Ok($"Int32CryptoId id = {id.Value}");
    }

    public record GetItem3Request([BindRequired] Int32CryptoId id, [BindRequired] int optional);

    /// <summary>
    /// Automatic binding and model state validation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("GetItem3")]
    public IActionResult GetItem3([FromQuery] GetItem3Request request)
    {
        if (!HttpContext.Request.Query.ContainsKey(nameof(request.id)))
        {
            Console.WriteLine($"Hasn't query paramer - {nameof(request.id)}");
        }
        if (!HttpContext.Request.Query.ContainsKey(nameof(request.optional)))
        {
            Console.WriteLine($"Hasn't query paramer - {nameof(request.optional)}");
        }
        var isBindingAttemptedForId = ModelState.TryGetValue(nameof(request.id), out var _);
        var isBindingAttemptedForUnused = ModelState.TryGetValue(nameof(request.optional), out var _);
        return Ok($"Int32CryptoId id = {request.id.Value}");
    }

    /// <summary>
    /// Implicit binding to DI Int64 encoder
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("GetItem4")]
    public IActionResult GetItem4([ModelBinder(typeof(Int64Binder))] long id)
    {
        return Ok($"long id = {id}");
    }

    /// <summary>
    /// Default parameter binding through DI
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("GetItem5")]
    public IActionResult GetItem5(Int64CryptoId id)
    {
        return Ok($"Int64CryptoId id = {id.Value}");
    }

    /// <summary>
    /// Getting encoder from DI and decoding string
    /// </summary>
    /// <param name="id"></param>
    /// <param name="encoder"></param>
    /// <returns></returns>
    [HttpGet("GetItem6")]
    public IActionResult GetItem6(string id, [FromServices] ICryptoIdEncoder<long> encoder)
    {
        return Ok($"string id (encoded) = {id}, long id (decoded) = {encoder.Decode(id)}");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
