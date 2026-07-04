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

    private char[] _encodedBuffer = new char[128];

    private string _encodedStringAes = null!;
    private string _encodedStringSpeck64 = null!;
    private string _encodedStringSpeck32 = null!;
    private string _encodedStringSkip32 = null!;
    private string _encodedStringCompactDetAes = null!;
    private string _encodedStringDetAes = null!;
    private string _encodedStringDetChaCha20 = null!;

    private ICryptoIdEncoder<long> _encoderAes = null!;
    private ICryptoIdEncoder<long> _encoderSpeck64 = null!;
    private ICryptoIdEncoder<int> _encoderSpeck32 = null!;
    private ICryptoIdEncoder<int> _encoderSkip32 = null!;
    private ICryptoIdEncoder<long> _encoderCompactDetAes = null!;
    private ICryptoIdEncoder<long> _encoderDetAes = null!;
    private ICryptoIdEncoder<long> _encoderDetChaCha20 = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize Encoders
        _encoderSpeck64 = CryptoIdFactory.Create<long>(IdCipherAlgorithm.Speck64_128, "Hello World!");
        _encoderSpeck32 = CryptoIdFactory.Create<int>(IdCipherAlgorithm.Speck32_64, "Hello World!");

        _encoderSkip32 = CryptoIdFactory.Create<int>(IdCipherAlgorithm.Skip32, "Hello World!");

#pragma warning disable CS0618 // Type or member is obsolete
        _encoderAes = CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, "Hello World!");
#pragma warning restore CS0618 // Type or member is obsolete
        _encoderDetAes = CryptoIdFactory.Create<long>(IdCipherAlgorithm.DeterministicAesGcm, "Hello World!");
        _encoderCompactDetAes = CryptoIdFactory.Create<long>(IdCipherAlgorithm.CompactDeterministicAes, "Hello World!");

        _encoderDetChaCha20 = CryptoIdFactory.Create<long>(IdCipherAlgorithm.DeterministicChaCha20Poly1305, "Hello World!");

        // Generate strings for decode
        _encodedStringSpeck64 = _encoderSpeck64.Encode(Int64Id);
        _encodedStringSpeck32 = _encoderSpeck32.Encode(Int32Id);

        _encodedStringSkip32 = _encoderSkip32.Encode(Int32Id);

        _encodedStringAes = _encoderAes.Encode(Int64Id);
        _encodedStringDetAes = _encoderDetAes.Encode(Int64Id);
        _encodedStringCompactDetAes = _encoderCompactDetAes.Encode(Int64Id);

        _encodedStringDetChaCha20 = _encoderDetChaCha20.Encode(Int64Id);
    }

    [Benchmark]
    public string Encode_Speck64_128() => _encoderSpeck64.Encode(Int64Id);
    [Benchmark]
    public bool TryEncode_Speck64_128() => _encoderSpeck64.TryEncode(Int64Id, _encodedBuffer, out _);
    [Benchmark]
    public long Decode_Speck64_128() => _encoderSpeck64.Decode(_encodedStringSpeck64);

    [Benchmark]
    public string Encode_Speck32_64() => _encoderSpeck32.Encode(Int32Id);
    [Benchmark]
    public bool TryEncode_Speck32_64() => _encoderSpeck32.TryEncode(Int32Id, _encodedBuffer, out _);
    [Benchmark]
    public int Decode_Speck32_64() => _encoderSpeck32.Decode(_encodedStringSpeck32);

    [Benchmark]
    public string Encode_Skip32() => _encoderSkip32.Encode(Int32Id);
    [Benchmark]
    public bool TryEncode_Skip32() => _encoderSkip32.TryEncode(Int32Id, _encodedBuffer, out _);
    [Benchmark]
    public int Decode_Skip32() => _encoderSkip32.Decode(_encodedStringSkip32);

    [Benchmark(Baseline = true)]
    public string Encode_AesGcm() => _encoderAes.Encode(Int64Id);
    [Benchmark]
    public bool TryEncode_AesGcm() => _encoderAes.TryEncode(Int64Id, _encodedBuffer, out _);
    [Benchmark]
    public long Decode_AesGcm() => _encoderAes.Decode(_encodedStringAes);

    [Benchmark]
    public string Encode_DeterministicAesGcm() => _encoderDetAes.Encode(Int64Id);
    [Benchmark]
    public bool TryEncode_DeterministicAesGcm() => _encoderDetAes.TryEncode(Int64Id, _encodedBuffer, out _);
    [Benchmark]
    public long Decode_DeterministicAesGcm() => _encoderDetAes.Decode(_encodedStringDetAes);

    [Benchmark]
    public string Encode_CompactDeterministicAes() => _encoderCompactDetAes.Encode(Int64Id);
    [Benchmark]
    public bool TryEncode_CompactDeterministicAes() => _encoderCompactDetAes.TryEncode(Int64Id, _encodedBuffer, out _);
    [Benchmark]
    public long Decode_CompactDeterministicAes() => _encoderCompactDetAes.Decode(_encodedStringCompactDetAes);

    [Benchmark]
    public string Encode_DeterministicChaCha20Poly1305() => _encoderDetChaCha20.Encode(Int64Id);
    [Benchmark]
    public bool TryEncode_DeterministicChaCha20Poly1305() => _encoderDetChaCha20.TryEncode(Int64Id, _encodedBuffer, out _);
    [Benchmark]
    public long Decode_DeterministicChaCha20Poly1305() => _encoderDetChaCha20.Decode(_encodedStringDetChaCha20);
}
