## Xobex.CryptoId: Secure and Lightweight ID Encryption for .NET
Xobex.CryptoId is a high-performance .NET library designed to protect your database identifiers from enumeration attacks by encrypting sequential IDs into secure, URL-safe obfuscated tokens.
## Key Features

* Unified API: Implements a strict, generic contract for type-safe ID encoding and decoding.
* Advanced Encryption Standard: Uses AES-GCM for maximum cryptographic security.
* Lightweight Ciphers: Implements Speck-32/64 and Speck-64/128 for ultra-fast, low-overhead obfuscation.
* URL-Safe Output: Automatically converts encrypted IDs to and from URL-safe Base64 strings.
* Allocation Optimized: Uses modern .NET memory primitives like ReadOnlySpan<char> to minimize allocations during decoding.

------------------------------
## Supported Ciphers

| Cipher | Key Size | Block Size | Compatible Types | Best Used For |
|---|---|---|---|---|
| AES-GCM | 256-bit | 128-bit | int, long, etc. | High-security environments, web tokens, sensitive data. |
| Speck-64/128 | 128-bit | 64-bit | long, ulong | Balanced speed and security for standard 64-bit integer IDs. |
| Speck-32/64 | 64-bit | 32-bit | int, uint, short | Ultra-low latency, tiny payloads, 32-bit integer IDs. |

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
## Quick Start## 1. High-Security ID Masking (AES-GCM)
Recommended for public-facing web APIs where maximum cryptographic strength is required.

```csharp
using Xobex.CryptoId;
// Initialize the encoder with a secure 32-byte key
ICryptoIdEncoder<long> encoder = CryptoIdFactory.Create<long>(IdCiperAlgorithm.AesGcm, "your secret phrase");;
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
ICryptoIdEncoder<long> speckEncoder = CryptoIdFactory.Create<long>(IdCiperAlgorithm.Speck64_128, "your secret phrase");
long databaseId = 987654321;
// Extremely fast encryption and URL-safe encoding
string encodedToken = speckEncoder.Encode(databaseId);
// Fast decryptionlong
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
Would you like me to add a Benchmark section showing the speed difference between AES-GCM and Speck?

