using Xobex.Cryptography;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Tests;

/// <summary>
/// Tests for AesCryptoIdEncoder using AES-GCM encryption.
/// </summary>
[TestClass]
public class DeterministicAesGcmCryptoIdEncoderTests : CryptoIdTestBase
{
    [TestMethod]
    [Description("Constructor should accept valid key and salt")]
    public void Constructor_WithValidInputs_InitializesSuccessfully()
    {
        // Arrange & Act
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);

        // Assert - No exception thrown
        Assert.IsNotNull(encoder);
    }

    [TestMethod]
    [Description("Constructor should throw when key is null")]
    public void Constructor_WithNullKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            new DeterministicAesGcmCryptoIdEncoder(null!, TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("null", StringComparison.OrdinalIgnoreCase) ||
                     ex.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Constructor should throw when key is empty")]
    public void Constructor_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            new DeterministicAesGcmCryptoIdEncoder("", TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("null", StringComparison.OrdinalIgnoreCase) ||
                     ex.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Constructor should throw when salt is null")]
    public void Constructor_WithNullSalt_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        ThrowsException<ArgumentNullException>(() =>
            new DeterministicAesGcmCryptoIdEncoder(TestKey, null!)
        );
    }

    [TestMethod]
    [Description("Encode should produce Base64Url formatted output")]
    public void Encode_ProducesValidBase64UrlOutput()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        var testValue = 123456789L;

        // Act
        var encoded = encoder.Encode(testValue);

        // Assert
        Assert.IsNotNull(encoded);
        Assert.IsTrue(IsValidBase64Url(encoded), "Output should be valid Base64Url");
    }

    [TestMethod]
    [Description("Encode and Decode should round-trip successfully")]
    public void EncodeDecodeRoundTrip_ShouldRecoverOriginalValue()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        var testValue = 987654321L;

        // Act
        var encoded = encoder.Encode(testValue);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(testValue, decoded);
    }

    [TestMethod]
    [Description("Multiple encodes of same value should produce different ciphertexts (random nonce)")]
    [DataRow(0L)]
    [DataRow(1L)]
    [DataRow(123456789L)]
    [DataRow(long.MaxValue)]
    public void Encode_WithRandomNonce_ProducesDifferentCiphertexts(long value)
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded1 = encoder.Encode(value);
        var encoded2 = encoder.Encode(value);
        var encoded3 = encoder.Encode(value);

        // Assert - All should be different due to random nonce
        Assert.AreEqual(encoded1, encoded2, "Random nonce should produce different ciphertexts");
        Assert.AreEqual(encoded2, encoded3, "Random nonce should produce different ciphertexts");
        Assert.AreEqual(encoded1, encoded3, "Random nonce should produce different ciphertexts");
    }

    [TestMethod]
    [Description("Decode all randomly-encrypted values should recover original")]
    public void Encode_RandomNonce_CanBeDecodedCorrectly()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        var testValue = 555555L;

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            var encoded = encoder.Encode(testValue);
            var decoded = encoder.Decode(encoded);
            Assert.AreEqual(testValue, decoded, $"Iteration {i}: Round-trip failed");
        }
    }

    [TestMethod]
    [Description("Different plaintexts should produce different ciphertexts")]
    public void Encode_DifferentValues_ProduceDifferentCiphertexts()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        var value1 = 111111L;
        var value2 = 222222L;

        // Act
        var encoded1 = encoder.Encode(value1);
        var encoded2 = encoder.Encode(value2);

        // Assert
        Assert.AreNotEqual(encoded1, encoded2);
    }

    [TestMethod]
    [Description("Decode should throw on invalid Base64Url")]
    public void Decode_WithInvalidBase64Url_ThrowsFormatException()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);

        // Act & Assert
        ThrowsException<FormatException>(() =>
            encoder.Decode("invalid!!!base64")
        );
    }

    [TestMethod]
    [Description("Decode should throw on truncated ciphertext")]
    public void Decode_WithTruncatedCiphertext_ThrowsFormatException()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123456L);
        var truncated = encoded.Substring(0, Math.Max(1, encoded.Length - 5));

        // Act & Assert
        ThrowsException<FormatException>(() =>
            encoder.Decode(truncated)
        );
    }

    [TestMethod]
    [Description("Decode should throw when authentication fails (corrupted ciphertext)")]
    public void Decode_WithCorruptedCiphertext_ThrowsException()
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123456L);

        // Corrupt the ciphertext by changing a character
        var array = encoded.ToCharArray();
        array[encoded.Length - 1] = (char)((int)array[encoded.Length - 1] ^ 0xAA);
        var corrupted = new string(array);

        // Act & Assert
        ThrowsException<Exception>(() =>
            encoder.Decode(corrupted)
        );
    }

    [TestMethod]
    [Description("Encode-Decode with all edge case values")]
#pragma warning disable MSTEST0052 // Avoid passing an explicit 'DynamicDataSourceType' and use the default auto detect behavior
    [DynamicData(nameof(GetLongTestValuesData), DynamicDataSourceType.Method)]
#pragma warning restore MSTEST0052 // Avoid passing an explicit 'DynamicDataSourceType' and use the default auto detect behavior
    public void EncodeDecodeRoundTrip_WithEdgeCaseValues(long value)
    {
        // Arrange
        using var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(value);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(value, decoded, $"Round-trip failed for value: {value}");
        Assert.IsTrue(IsValidBase64Url(encoded), "Output should be valid Base64Url");
    }

    [TestMethod]
    [Description("Different keys should produce different ciphertexts")]
    public void Encode_DifferentKeys_ProduceDifferentCiphertexts()
    {
        // Arrange
        using var encoder1 = new DeterministicAesGcmCryptoIdEncoder("key-1", TestSalt);
        using var encoder2 = new DeterministicAesGcmCryptoIdEncoder("key-2", TestSalt);
        var testValue = 123456L;

        // Act - Get multiple encodings from each encoder (for statistical difference, not single comparison)
        var encoded1 = encoder1.Encode(testValue);
        var encoded2 = encoder2.Encode(testValue);

        // Assert
        Assert.AreNotEqual(encoded1, encoded2, "Different keys should produce different ciphertexts");
    }

    [TestMethod]
    [Description("Different salts should produce different ciphertexts")]
    public void Encode_DifferentSalts_ProduceDifferentCiphertexts()
    {
        // Arrange
        var salt2 = Convert.FromHexString("ffffffffffffffffffffffffffffffff");
        using var encoder1 = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);
        using var encoder2 = new DeterministicAesGcmCryptoIdEncoder(TestKey, salt2);
        var testValue = 123456L;

        // Act
        var encoded1 = encoder1.Encode(testValue);
        var encoded2 = encoder2.Encode(testValue);

        // Assert
        Assert.AreNotEqual(encoded1, encoded2, "Different salts should produce different ciphertexts");
    }

    [TestMethod]
    [Description("Dispose should work correctly")]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt);

        // Act & Assert - Should not throw
        encoder.Dispose();
    }

    [TestMethod]
    [Description("Using statement should properly dispose")]
    public void Using_ShouldCallDispose()
    {
        // Arrange & Act
        using (var encoder = new DeterministicAesGcmCryptoIdEncoder(TestKey, TestSalt))
        {
            var encoded = encoder.Encode(123L);
            Assert.IsNotNull(encoded);
        }
    }

    // Helper method for DataRow generation
    public static IEnumerable<object[]> GetLongTestValuesData()
    {
        foreach (var value in GetLongTestValues())
        {
            yield return new object[] { value };
        }
    }
}
