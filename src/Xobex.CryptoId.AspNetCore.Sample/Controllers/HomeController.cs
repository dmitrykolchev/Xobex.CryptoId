// <copyright file="HomeController.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Xobex.CryptoId.AspNetCore.ModelBinding;
using Xobex.CryptoId.AspNetCore.Sample.Models;

namespace Xobex.CryptoId.AspNetCore.Sample.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("GetItem1")]
    public IActionResult GetItem1([ModelBinder(typeof(Int32Binder))]int id)
    {
        return Ok(id);
    }

    [HttpGet("GetItem2")]
    public IActionResult GetItem2(Int32CryptoId id)
    {
        return Ok(id.Value);
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
