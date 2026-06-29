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

> Do not use for cryptographic protection!


------------------------------
## Supported Ciphers

| Cipher | Key Size | Block Size | Compatible Types | Best Used For |
|---|---|---|---|---|
| AES-GCM | 256-bit | 128-bit | int, long, etc. | High-security environments, web tokens, sensitive data. |
| Speck-64/128 | 128-bit | 64-bit | long, ulong | Balanced speed and security for standard 64-bit integer IDs. |
| Speck-32/64 | 64-bit | 32-bit | int, uint, short | Ultra-low latency, tiny payloads, 32-bit integer IDs. |
| Skip32 | 80-bit | 32-bit | int, uint, short | Low latency, tiny payloads, 32-bit integer IDs. |

------------------------------
## Core Abstraction
All implementations share a single unified interface, making it easy to swap cryptographic algorithms without changing your business logic:

```csharp
public interface ICryptoIdEncoder<T> where T : struct
{
    string Encode(T id);
    T Decode(ReadOnlySpan<char> urlEncodedBase64);
}
```

------------------------------
## Installation
Install the package via NuGet Package Manager Console:

```pwsh
Install-Package Xobex.CryptoId
```

Or via the .NET CLI:

```bash
dotnet add package Xobex.CryptoId
```

------------------------------
## Quick Start

### Adding to the DI container

```csharp
    builder.Services.AddSingleton<ICryptoIdEncoder<int>>(serviceProvider =>
    {
        return CryptoIdFactory.Create<int>(IdCipherAlgorithm.Speck32_64, "my_strong_secret");
    });
```

### Encoding database IDs

```csharp
public sealed class LibraryDataService : DataServiceBase
{
    private readonly ICryptoIdEncoder<int> _int32encoder;

    public LibraryDataService(PhotoDbContextBase context, ICryptoIdEncoder<int> int32encoder) : base(context)
    {
        ArgumentNullException.ThrowIfNull(int32encoder);
        _int32encoder = int32encoder;
    }

    public async Task<List<LibraryViewEntry>> GetLibrariesAsync(CancellationToken cancellation = default)
    {
        var query = from item in Context.Library.AsNoTracking()
                    select new LibraryViewEntry()
                    {
                        Id = _int32encoder.Encode(item.Id),
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
    public async Task<LibraryViewEntry> UpdateLibraryAsync(string id, UpdateLibraryRequest request, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(request.Name);
        var nId = _int32encoder.Decode(id);
        var found = await Context.Library.SingleAsync(t => t.Id == nId, cancellation).ConfigureAwait(false);
        ...
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
## License
This project is licensed under the MIT License - see the LICENSE.TXT file for details.
------------------------------

## Benchmark Results

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i7-10700KF CPU 3.80GHz (Max: 3.79GHz), 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3


```
| Method             | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| Encode_AesGcm      | 270.88 ns | 1.214 ns | 1.076 ns |  1.00 | 0.0143 |     120 B |        1.00 |
| Encode_Speck64_128 |  32.06 ns | 0.107 ns | 0.100 ns |  0.12 | 0.0057 |      48 B |        0.40 |
| Encode_Speck32_64  |  39.37 ns | 0.205 ns | 0.171 ns |  0.15 | 0.0048 |      40 B |        0.33 |
| Encode_Skip32      | 194.34 ns | 1.024 ns | 0.958 ns |  0.72 | 0.0048 |      40 B |        0.33 |
| Decode_AesGcm      | 187.59 ns | 1.643 ns | 1.457 ns |  0.69 |      - |         - |        0.00 |
| Decode_Speck64_128 |  43.04 ns | 0.133 ns | 0.111 ns |  0.16 |      - |         - |        0.00 |
| Decode_Speck32_64  |  62.31 ns | 0.253 ns | 0.236 ns |  0.23 |      - |         - |        0.00 |
| Decode_Skip32      | 197.91 ns | 1.893 ns | 1.771 ns |  0.73 |      - |         - |        0.00 |
