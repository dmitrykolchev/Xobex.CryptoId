## Xobex.CryptoId: Secure and Lightweight ID Encryption for .NET

Xobex.CryptoId is a high-performance .NET library designed to protect your database identifiers from enumeration attacks by encrypting sequential IDs into secure, URL-safe obfuscated tokens. In simple words Xobex.CryptoId is reversible ID obfuscator.

<img width="630" height="181" alt="image" src="https://github.com/user-attachments/assets/d696c528-6fa1-4809-b196-ce1dacf883bd" />

## Key Features

* Unified API: Implements a strict, generic contract for type-safe ID encoding and decoding.
* Advanced Encryption Standard: Uses AES-GCM for maximum cryptographic security.
* Lightweight Ciphers: Implements Speck-32/64 and Speck-64/128 for ultra-fast, low-overhead obfuscation.
* Low latency Skip32 obfuscator
* URL-Safe Output: Automatically converts encrypted IDs to and from URL-safe Base64 strings.
* Allocation Optimized: Uses modern .NET memory primitives like ReadOnlySpan<char> to minimize allocations during decoding.

## Skip32, Speck-32/64, Speck-64/128 Notes
Use only for obfuscation of sequential IDs in public URLs/APIs.
Do not use for cryptographic protection!


------------------------------
## Supported Ciphers

| Cipher | Key Size | Block Size | Compatible Types | Encoded Length | Best Used For |
|---|---|---|---|---|---|
| Nondeterministic AES-GCM | 256-bit | 128-bit | int, long, etc. | 48 chars | High-security environments, web tokens, sensitive data. |
| Deterministic AES-GCM | 256-bit | 128-bit | int, long, etc. | 48 chars | High-security environments, web tokens, sensitive data. |
| Compact Deterministic AES-ECB | 256-bit | 128-bit | int, long, etc.| 22 chars | High-security environments, web tokens, sensitive data. |
| Deterministic ChaCha20-Poly1305 | 256-bit | 128-bit | int, long, etc. | 48 chars | High-security environments, web tokens, sensitive data. |
| Speck-64/128 | 128-bit | 64-bit | long, ulong | 11 chars | Balanced speed and security for standard 64-bit integer IDs. |
| Speck-32/64 | 64-bit | 32-bit | int, uint, short | 6 chars | Ultra-low latency, tiny payloads, 32-bit integer IDs. |
| Skip32 | 80-bit | 32-bit | int, uint, short | 6 chars | Low latency, tiny payloads, 32-bit integer IDs. |

------------------------------
## Core Abstraction
All implementations share a single unified interface, making it easy to swap cryptographic algorithms without changing your business logic:

```csharp
/// <summary>
/// Defines the contract for encrypting and decrypting cryptographic identifiers of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The data type of the identifier to be encoded/decoded. Must be a value type.</typeparam>
/// <remarks>
/// Implementations of this interface should provide encryption and decryption operations that convert
/// identifiers to and from URL-safe Base64 strings, suitable for use in web URLs and APIs.
/// </remarks>
public interface ICryptoIdEncoder<T> where T : struct
{
    /// <summary>
    /// Encrypts an identifier and encodes it to a URL-safe Base64 string.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <returns>The encrypted identifier as a URL-safe Base64 encoded string.</returns>
    string Encode(T id);

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <returns>The decrypted identifier.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid URL-safe Base64 string or contains invalid data.</exception>
    T Decode(ReadOnlySpan<char> urlEncodedBase64);

    /// <summary>
    /// Gets a value indicating whether the encryption is deterministic (i.e., the same input always produces the same output).
    /// </summary>
    public bool IsDeterministic { get; }

    /// <summary>
    /// Attempts to encode an identifier into a provided character span, returning a boolean indicating success or failure.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="destination">The span of characters to write the encoded identifier to.</param>
    /// <param name="charsWritten">When the method returns, contains the number of characters written to the destination span.</param>
    /// <returns>true if the identifier was successfully encoded; otherwise, false.</returns>
    bool TryEncode(T id, Span<char> destination, out int charsWritten);

    /// <summary>
    /// Gets the type of the identifier that this encoder can process.
    /// </summary>
    Type IdType { get; }

    /// <summary>
    /// Gets the size of the identifier in bytes.
    /// </summary>
    int IdSizeInBytes { get; }
}
```

------------------------------
## Installation
Install the package via NuGet Package Manager Console:

```pwsh
Install-Package Xobex.CryptoId
Install-Package Xobex.CryptoId.AspNetCore
```

Or via the .NET CLI:

```bash
dotnet add package Xobex.CryptoId
dotnet add package Xobex.CryptoId.AspNetCore
```

------------------------------
## Quick Start

### Adding to the DI container

```csharp
    var builder = WebApplication.CreateBuilder(args);
    var cryptoIdOptions = new CryptoIdOptions();
    builder.Services.AddCryptoId(cryptoIdOptions);
```

### Encoding database IDs

```csharp
public sealed class LibraryDataService : DataServiceBase
{
    public LibraryDataService(PhotoDbContextBase context) : base(context)
    {
    }

    public async Task<List<LibraryViewEntry>> GetLibrariesAsync(CancellationToken cancellation = default)
    {
        var query = from item in Context.Library.AsNoTracking()
                    select new LibraryViewEntry()
                    {
                        Id = (Int64CryptoId)item.Id,
                        State = item.State,
                        Name = item.Name,
                        Path = item.Path,
                        LastScanDate = item.LastScanDate,
                        Created = item.Created,
                        Modified = item.Modified,
                        ImageCount = item.Image.Count
                    };

        return await query.ToListAsync(cancellation).ConfigureAwait(false);
    }
    ...
```

### Decoding database IDs

```csharp

    // used implicit model binder to decode Int64CryptoId from URL-safe Base64 string
    [HttpGet("GetItem3")]
    public IActionResult GetItem3([ModelBinder(typeof(Int64Binder))] long id)
    {
        return Ok($"long id = {id}");
    }

    // used type specific model binder to decode Int64CryptoId from URL-safe Base64 string
    [HttpGet("GetItem4")]
    public IActionResult GetItem4(Int64CryptoId id)
    {
        return Ok($"Int64CryptoId id = {id.Value}");
    }

    // used service injection to decode Int64CryptoId from URL-safe Base64 string
    [HttpGet("GetItem5")]
    public IActionResult GetItem5(string id, [FromServices] ICryptoIdEncoder<long> encoder)
    {
        return Ok($"string id (encoded) = {id}, long id (decoded) = {encoder.Decode(id)}");
    }
```


## 1. High-Security ID Masking (AES-GCM)
Recommended for public-facing web APIs where maximum cryptographic strength is required.

```csharp
using Xobex.CryptoId;
// Initialize the encoder with a secure 32-byte key
ICryptoIdEncoder<long> encoder = CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, "your secret phrase");
long originalId = 42026;
// Encrypt the ID to a URL-safe Base64 string
string encodedToken = encoder.Encode(originalId); 
Console.WriteLine($"Encoded: {encodedToken}"); // Output looks like: "A3fG...8x"
// Decrypt back to the original ID using ReadOnlySpan<char>
long decodedId = encoder.Decode(encodedToken);
Console.WriteLine($"Decoded: {decodedId}"); // Output: 42026
```

## 2. Ultra-Lightweight ID Obfuscation (Speck)
Recommended for internal microservices, high-throughput systems, or scenarios requiring millions of operations per second with minimal memory allocation.

```csharp
using Xobex.CryptoId;
// Initialize Speck-64/128 (16-byte key) for 64-bit long ID;
ICryptoIdEncoder<long> speckEncoder = CryptoIdFactory.Create<long>(IdCipherAlgorithm.Speck64_128, "your secret phrase");
long databaseId = 987654321;
// Extremely fast encryption and URL-safe encoding
string encodedToken = speckEncoder.Encode(databaseId);
// Fast decryption
long restoredId = speckEncoder.Decode(encodedToken);
```
------------------------------
## Error Handling
The Decode method throws a FormatException if the provided string is not valid URL-safe Base64 or if the decrypted data structure is corrupted or invalid.

```csharp
try
{
    long id = encoder.Decode("invalid_token_here");
}
catch (FormatException ex)
{
    // Handle invalid token or tampering attempt
    logger.LogWarning("Failed to decode identifier: " + ex.Message);
}
```
------------------------------
## Benchmark Results

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i7-10700KF CPU 3.80GHz (Max: 3.79GHz), 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3


```
| Method                                  | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Encode_Speck64_128                      |    32.57 ns | 0.317 ns | 0.296 ns |  0.12 |    0.00 | 0.0057 |      48 B |        0.40 |
| TryEncode_Speck64_128                   |    29.69 ns | 0.290 ns | 0.257 ns |  0.11 |    0.00 |      - |         - |        0.00 |
| Decode_Speck64_128                      |    42.99 ns | 0.100 ns | 0.093 ns |  0.16 |    0.00 |      - |         - |        0.00 |
| Encode_Speck32_64                       |    39.50 ns | 0.132 ns | 0.110 ns |  0.15 |    0.00 | 0.0048 |      40 B |        0.33 |
| TryEncode_Speck32_64                    |    35.69 ns | 0.320 ns | 0.283 ns |  0.13 |    0.00 |      - |         - |        0.00 |
| Decode_Speck32_64                       |    61.96 ns | 0.139 ns | 0.130 ns |  0.23 |    0.00 |      - |         - |        0.00 |
| Encode_Skip32                           |   194.99 ns | 1.318 ns | 1.169 ns |  0.73 |    0.01 | 0.0048 |      40 B |        0.33 |
| TryEncode_Skip32                        |   190.14 ns | 0.498 ns | 0.466 ns |  0.71 |    0.00 |      - |         - |        0.00 |
| Decode_Skip32                           |   195.76 ns | 0.573 ns | 0.536 ns |  0.73 |    0.00 |      - |         - |        0.00 |
| Encode_AesGcm                           |   268.95 ns | 1.734 ns | 1.622 ns |  1.00 |    0.01 | 0.0143 |     120 B |        1.00 |
| TryEncode_AesGcm                        |   253.05 ns | 1.007 ns | 0.942 ns |  0.94 |    0.01 |      - |         - |        0.00 |
| Decode_AesGcm                           |   190.49 ns | 0.585 ns | 0.547 ns |  0.71 |    0.00 |      - |         - |        0.00 |
| Encode_DeterministicAesGcm              |   975.23 ns | 4.063 ns | 3.601 ns |  3.63 |    0.02 | 0.0134 |     120 B |        1.00 |
| TryEncode_DeterministicAesGcm           |   966.29 ns | 1.213 ns | 1.076 ns |  3.59 |    0.02 |      - |         - |        0.00 |
| Decode_DeterministicAesGcm              |   189.76 ns | 0.423 ns | 0.396 ns |  0.71 |    0.00 |      - |         - |        0.00 |
| Encode_CompactDeterministicAes          |   414.64 ns | 1.760 ns | 1.560 ns |  1.54 |    0.01 | 0.0191 |     160 B |        1.33 |
| TryEncode_CompactDeterministicAes       |   398.34 ns | 2.117 ns | 1.980 ns |  1.48 |    0.01 | 0.0105 |      88 B |        0.73 |
| Decode_CompactDeterministicAes          |   418.28 ns | 3.039 ns | 2.843 ns |  1.56 |    0.01 | 0.0105 |      88 B |        0.73 |
| Encode_DeterministicChaCha20Poly1305    | 1,140.01 ns | 3.412 ns | 3.192 ns |  4.24 |    0.03 | 0.0134 |     120 B |        1.00 |
| TryEncode_DeterministicChaCha20Poly1305 | 1,119.34 ns | 2.982 ns | 2.789 ns |  4.16 |    0.03 |      - |         - |        0.00 |
| Decode_DeterministicChaCha20Poly1305    |   336.74 ns | 0.543 ns | 0.453 ns |  1.25 |    0.01 |      - |         - |        0.00 |

------------------------------
## License
This project is licensed under the MIT License - see the LICENSE.TXT file for details.
------------------------------

