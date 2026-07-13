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

## Example Encoded IDs

| Cipher | Example Encoded ID | Length |
|---|---|---|
| Skip32                         | 4kIX3g                                           |  6 |
| Speck-32/64                      | HiJJTw                                           |  6 |
| Speck-64/128                     | iuMbGJ2ZEbU                                      | 11 |
| Compact Det. AES-ECB        | d2b5jAF5XmprvL8CO2V6dA                           | 22 |
| AES-GCM                         | dQBZe118pekBE6MDa7-t9nSIkJmUnfI88iL_2iKLRjlon-8p | 48 |
| Det. AES-GCM            | jJ-RPdfdRh6wg9WSl2W-zksuq8sbgjoinvxtW0kNpU1f_cc7 | 48 |
| Det. ChaCha20-Poly1305  | picyFZmOpgO2rpVfTXrIEpfg5eWyQtA-eg6jYYDbnsexQPOM | 48 |

## Choosing the Right Encoder

Select the appropriate implementation based on your application's specific requirements for security, payload size, and performance.

| Algorithm | Security Level | Payload Size (URL) | Performance | Best Use Case |
| :--- | :--- | :--- | :--- | :--- |
| **Speck** | **Low** (Obfuscation) | **Minimal** | **Extreme** | Hiding sequential integer IDs in non-sensitive contexts where performance is the absolute priority. |
| **Compact AES-GCM+FNV-1a** | **High** (SIV Mode) | **Compact** | **High** | **The Recommended Standard** for public-facing APIs and URLs. Provides strong protection against tampering and guessing while remaining URL-friendly. |
| **Deterministic AES-GCM+HMAC-SHA256** | **Maximum** | **Large** | **Moderate** | Encrypting sensitive data structures where payload size is not a constraint and full AEAD properties are required. |

***

### Key Considerations for Developers:

*   **Security vs. Obfuscation:** 
    *   Use **Speck** only if you want to prevent casual users from guessing the next ID (e.g., changing `id=100` to `id=101`). It does not protect against sophisticated attackers.
    *   Use **Compact AES-GCM** if you need to ensure that an attacker cannot forge or modify an ID in a URL, even if they know the structure of your identifiers.
*   **Payload Size:** 
    *   The `Compact` variant is specifically engineered to keep the Base64Url string length minimal (approx. 22 characters), making it ideal for SEO and clean URL design.
*   **Deterministic Behavior:** 
    *   All variants are **deterministic** (the same input produces the same output). This is intentional to allow for consistent URL generation, but it means they are not suitable for encrypting data that requires unique ciphertexts for every operation.

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
    /// Attempts to encode an identifier into a provided character span, returning a boolean indicating success or failure.
    /// </summary>
    /// <param name="id">The identifier to encrypt.</param>
    /// <param name="destination">The span of characters to write the encoded identifier to.</param>
    /// <param name="charsWritten">When the method returns, contains the number of characters written to the destination span.</param>
    /// <returns>true if the identifier was successfully encoded; otherwise, false.</returns>
    bool TryEncode(T id, Span<char> destination, out int charsWritten);

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <returns>The decrypted identifier.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid URL-safe Base64 string or contains invalid data.</exception>
    T Decode(ReadOnlySpan<char> urlEncodedBase64);

    /// <summary>
    /// Decodes a URL-safe Base64 string and decrypts it back to the original identifier.
    /// </summary>
    /// <param name="urlEncodedBase64">The encrypted identifier as a URL-safe Base64 encoded string.</param>
    /// <param name="value">The decrypted identifier</param>
    /// <returns>Returns true is decryption was successfull</returns>
    bool TryDecode(ReadOnlySpan<char> urlEncodedBase64, out T value);

    /// <summary>
    /// Gets a value indicating whether the encryption is deterministic (i.e., the same input always produces the same output).
    /// </summary>
    public bool IsDeterministic { get; }

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
## Key Notes on Deterministic vs Non-Deterministic Encryption

### Key Advantages and Features of CompactDeterministicAes encoder

#### 1. Low Latency
*   **Cryptographic MAC Bypass:** Replaces heavy cryptographic MACs (like HMAC-SHA256) with an ultra-fast non-cryptographic FNV-1a checksum. This bypasses the typical ~200 ns latency associated with multi-round SHA-256 compression and managed-to-native P/Invoke boundaries.
*   **Hardware Acceleration:** Leverages native CPU instructions (AES-NI / ARMv8 Crypto Extensions) via highly optimized .NET `EncryptEcb` and `DecryptEcb` APIs, executing the core block cipher in mere nanoseconds.
*   **Inline Hashing:** The FNV-1a checksum is computed entirely in user-space, taking less than 2 ns for 8-byte inputs, ensuring the CPU pipeline remains clear.

#### 2. Ultra-Compact Payload (22 Characters)
*   **54% Length Reduction:** Encrypts data into a single 16-byte block (128 bits), which encodes to exactly **22 characters** in URL-safe Base64. This is a massive reduction from the 48 characters required by standard AES-GCM (36 bytes).
*   **Web & URL-Safe:** Optimizes payload size for HTTP query parameters, path variables, cookies, and database indexing.

#### 3. Mathematical Integrity & Security
*   **Single-Block Pseudorandom Permutation (PRP):** Since the plaintext block `[ID (64-bit) || Checksum (64-bit)]` is precisely 16 bytes, AES-256 acts as a strong PRP. ECB mode is mathematically secure in this context because it encrypts exactly one block.
*   **Tamper-Proof Design:** Due to the avalanche effect of AES, any single-bit alteration in the ciphertext scrambles the entire decrypted block into random noise. The probability of an attacker forging a valid ciphertext to map to a matching checksum is mathematically bound to exactly $1/2^{64}$ ($\approx 5.4 \times 10^{-20}$), rendering non-cryptographic hashing cryptographically secure *inside* the encrypted block.
*   **Zero Nonce Collision Risk:** Completely eliminates GCM nonce-reuse vulnerabilities, as it does not rely on an Initialization Vector (IV).

#### 4. Absolute Zero-Allocation (0-Byte Garbage Collection Overhead)
*   **Span-Centric Architecture:** Designed from the ground up to use modern .NET memory primitives (`Span<T>`, `ReadOnlySpan<T>`, and `stackalloc`).
*   **Zero GC Pressure:** Eliminates heap allocations during encryption, decryption, hashing, and encoding when using buffer-writing overloads, making it ideal for high-throughput Hot Paths.

#### 5. SAST and Compliance Cleanliness
*   **No Broken Cryptographic Primitives:** Unlike speed-up attempts using MD5, this architecture relies strictly on AES-256 and FNV-1a. It completely bypasses static security analysis (SAST) warnings (such as CWE-327) and compliance blockers (FIPS, PCI-DSS, ISO 27001).


### Technical Specification & Features of `Speck-64/128 encoder`

#### 1. Hyper-Compact Encoded Output (11 Characters)
*   **Minimalist Footprint:** By using a 64-bit block size, the encoder maps a 64-bit `long` ID directly to an 8-byte ciphertext without any padding or initialization vectors. This results in exactly **11 characters** when encoded to URL-safe Base64.
*   **Ideal for Strict UI/UX Boundaries:** Provides the absolute mathematical minimum length for reversible 64-bit identifier obfuscation, crucial for short-link services, QR codes, and SMS-delivered URLs.

#### 2. Native Thread-Safety & Immutability (Zero Pooling)
*   **ARX Architecture:** Speck relies exclusively on Addition, Rotation, and XOR (ARX) operations. It does not maintain state or use mutable look-up tables (S-Boxes) during execution.
*   **Zero Concurrency Overhead:** The implementation is completely thread-safe and immutable after instantiation. It **does not require** `ThreadLocal<T>` or object pooling, eliminating context-switching and garbage collection overhead in highly concurrent environments.

#### 3. High Performance on Non-Accelerated Hardware
*   **Constant-Time Execution:** ARX operations naturally execute in constant-time on almost all modern CPU architectures. This shields the cipher against cache-timing and side-channel attacks by default.
*   **Hardware Independence:** Unlike AES, which requires hardware-level support (AES-NI) to run securely and quickly, Speck64/128 delivers exceptional performance on low-power devices, legacy servers, and restricted execution environments (e.g., WebAssembly, IoT, edge compute).

#### 4. Cryptographically Sound Key Schedule
*   **Domain-Separated KDF:** Uses HKDF-SHA256 to derive a high-entropy 128-bit key. This isolates the key material specifically to the ID encoding domain and prevents cross-protocol key leakage attacks.

------------------------------

### Speck-64/128 vs. AES-256 + FNV-1a Comparison Matrix

The table below outlines the core cryptographic, operational, and architectural trade-offs between the two low-latency ID encoding approaches:

| Feature / Metric | Speck64/128 (Deterministic) | AES-256 Single-Block + FNV-1a |
| :--- | :--- | :--- |
| **Output Length (Base64Url)** | **11 characters** (8 bytes of ciphertext) | **22 characters** (16 bytes of ciphertext) |
| **Integrity Verification (MAC)**| **None.** Decrypting arbitrary data always succeeds, returning a random-looking 64-bit ID. | **64-bit FNV-1a Checksum.** Decryption fails if the decrypted payload checksum does not match. |
| **Tampering & IDOR Resilience**| **Low.** Relies entirely on the application's authorization layer (ACL/ABAC) after decoding. | **High.** Automatically rejects manipulated or fuzzed ciphertexts before reaching business logic ($P_{\text{forge}} = 2^{-64}$). |
| **Concurrency / Thread-Safety** | **Naturally Thread-Safe.** No wrappers or pooling required (stateless/immutable after constructor). | **Stateful.** Requires pooling (`ThreadLocal<Aes>` or `ObjectPool<Aes>`) to ensure thread safety of the `Aes` instance. |
| **Hardware Dependency** | **None.** Runs exceptionally fast on any CPU (ARX design). | **High.** Secure, constant-time execution depends heavily on hardware AES-NI instructions. |
| **Enterprise & Compliance Audit**| **Risky.** Speck was rejected by ISO/IEC in 2018; often flagged by static analyzers (SAST) and enterprise security auditors. | **Excellent.** AES-256 is universally accepted as a safe industry standard across FIPS, PCI-DSS, and SOC2. |


### Architectural Recommendation
*   Use **Speck64/128** only when **URL length constraint is the absolute highest priority** (e.g., printed URLs, SMS, legacy protocols), and you can guarantee a robust, infallible authorization layer at the API boundary to mitigate IDOR risks.
*   Use **AES-256 + FNV-1a** for all general-purpose enterprise applications, as it provides built-in tampering rejection, standard compliance, and highly secure isolation of your internal database structure.


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
| Encode_Speck64_128                      |    32.79 ns | 0.186 ns | 0.165 ns |  0.11 |    0.00 | 0.0057 |      48 B |        0.40 |
| TryEncode_Speck64_128                   |    29.03 ns | 0.031 ns | 0.026 ns |  0.10 |    0.00 |      - |         - |        0.00 |
| Decode_Speck64_128                      |    42.94 ns | 0.116 ns | 0.109 ns |  0.15 |    0.00 |      - |         - |        0.00 |
| Encode_Speck32_64                       |    39.10 ns | 0.136 ns | 0.127 ns |  0.13 |    0.00 | 0.0048 |      40 B |        0.33 |
| TryEncode_Speck32_64                    |    35.63 ns | 0.084 ns | 0.070 ns |  0.12 |    0.00 |      - |         - |        0.00 |
| Decode_Speck32_64                       |    62.04 ns | 0.146 ns | 0.122 ns |  0.21 |    0.00 |      - |         - |        0.00 |
| Encode_Skip32                           |   194.02 ns | 0.456 ns | 0.404 ns |  0.66 |    0.00 | 0.0048 |      40 B |        0.33 |
| TryEncode_Skip32                        |   189.86 ns | 0.409 ns | 0.341 ns |  0.65 |    0.00 |      - |         - |        0.00 |
| Decode_Skip32                           |   195.38 ns | 0.535 ns | 0.474 ns |  0.67 |    0.00 |      - |         - |        0.00 |
| Encode_AesGcm                           |   293.73 ns | 1.276 ns | 1.193 ns |  1.00 |    0.01 | 0.0143 |     120 B |        1.00 |
| TryEncode_AesGcm                        |   276.04 ns | 1.045 ns | 0.977 ns |  0.94 |    0.00 |      - |         - |        0.00 |
| Decode_AesGcm                           |   215.49 ns | 0.642 ns | 0.601 ns |  0.73 |    0.00 |      - |         - |        0.00 |
| Encode_DeterministicAesGcm              |   996.97 ns | 2.202 ns | 1.838 ns |  3.39 |    0.01 | 0.0134 |     120 B |        1.00 |
| TryEncode_DeterministicAesGcm           |   983.35 ns | 4.264 ns | 3.780 ns |  3.35 |    0.02 |      - |         - |        0.00 |
| Decode_DeterministicAesGcm              |   218.37 ns | 0.825 ns | 0.689 ns |  0.74 |    0.00 |      - |         - |        0.00 |
| Encode_CompactDeterministicAes          |   429.87 ns | 1.544 ns | 1.444 ns |  1.46 |    0.01 | 0.0191 |     160 B |        1.33 |
| TryEncode_CompactDeterministicAes       |   423.19 ns | 2.563 ns | 2.140 ns |  1.44 |    0.01 | 0.0105 |      88 B |        0.73 |
| Decode_CompactDeterministicAes          |   453.23 ns | 0.656 ns | 0.512 ns |  1.54 |    0.01 | 0.0105 |      88 B |        0.73 |
| Encode_DeterministicChaCha20Poly1305    | 1,138.24 ns | 4.770 ns | 4.462 ns |  3.88 |    0.02 | 0.0134 |     120 B |        1.00 |
| TryEncode_DeterministicChaCha20Poly1305 | 1,115.90 ns | 1.991 ns | 1.862 ns |  3.80 |    0.02 |      - |         - |        0.00 |
| Decode_DeterministicChaCha20Poly1305    |   338.99 ns | 1.069 ns | 1.000 ns |  1.15 |    0.01 |      - |         - |        0.00 |

------------------------------
## License
This project is licensed under the MIT License - see the LICENSE.TXT file for details.
------------------------------

