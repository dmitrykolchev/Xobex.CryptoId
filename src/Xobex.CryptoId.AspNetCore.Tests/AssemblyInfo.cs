// <copyright file="AssemblyInfo.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.VisualStudio.TestTools.UnitTesting;

// Integration tests share a single WebApplicationFactory host and the process-wide
// CryptoIdRegistry static state, so tests must run sequentially.
[assembly: DoNotParallelize]
