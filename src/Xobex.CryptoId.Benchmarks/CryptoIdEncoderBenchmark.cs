// <copyright file="" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using BenchmarkDotNet.Attributes;
using Xobex.Cryptography.Abstractions;
using Xobex.Cryptography;

namespace Xobex.CryptoId.Benchmarks;

[MemoryDiagnoser] 
//[HideColumns("Job", "RatioSD", "AllocRatio")]
public class CryptoIdEncoderBenchmark
{
    private const long Int64Id = 12345678901234L;
    private const int Int32Id = 12345;

    private string _encodedString1 = null!;
    private string _encodedString2 = null!;
    private string _encodedString3 = null!;

    private ICryptoIdEncoder<long> _encoder1 = null!;
    private ICryptoIdEncoder<long> _encoder2 = null!;
    private ICryptoIdEncoder<int> _encoder3 = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize Encoders
        _encoder1 =  CryptoIdFactory.Create<long>(IdCiperAlgorithm.AesGcm, "Hello World!");
        _encoder2 = CryptoIdFactory.Create<long>(IdCiperAlgorithm.Speck64_128, "Hello World!");
        _encoder3 = CryptoIdFactory.Create<int>(IdCiperAlgorithm.Speck32_64, "Hello World!");

        // Generate strings for decode
        _encodedString1 = _encoder1.Encode(Int64Id);
        _encodedString2 = _encoder2.Encode(Int64Id);
        _encodedString3 = _encoder3.Encode(Int32Id);
    }

    [Benchmark(Baseline = true)]
    public string Encode_AesGcm() => _encoder1.Encode(Int64Id);

    [Benchmark]
    public string Encode_Speck64_128() => _encoder2.Encode(Int64Id);

    [Benchmark]
    public string Encode_Speck32_64() => _encoder3.Encode(Int32Id);

    [Benchmark]
    public long Decode_AesGcm() => _encoder1.Decode(_encodedString1);

    [Benchmark]
    public long Decode_Speck64_128() => _encoder2.Decode(_encodedString2);

    [Benchmark]
    public int Decode_Speck32_64() => _encoder3.Decode(_encodedString3);
}