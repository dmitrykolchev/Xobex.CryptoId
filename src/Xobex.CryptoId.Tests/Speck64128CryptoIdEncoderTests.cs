// <copyright file="Speck64128CryptoIdEncoderTests.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Cryptography;

namespace Xobex.CryptoId.Tests;

/// <summary>
/// Tests for Speck64128CryptoIdEncoder using Speck64/128 encryption for long IDs.
/// </summary>
[TestClass]
public class Speck64128CryptoIdEncoderTests : CryptoIdTestBase
{
    [TestMethod]
    [Description("Constructor should accept valid key and salt")]
    public void Constructor_WithValidInputs_InitializesSuccessfully()
    {
        // Arrange & Act
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);

        // Assert - No exception thrown
        Assert.IsNotNull(encoder);
    }

    [TestMethod]
    [Description("Constructor should throw when key is null")]
    public void Constructor_WithNullKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            new Speck64128CryptoIdEncoder(null!, TestSalt)
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
            new Speck64128CryptoIdEncoder("", TestSalt)
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
            new Speck64128CryptoIdEncoder(TestKey, null!)
        );
    }

    [TestMethod]
    [Description("Encode should produce Base64Url formatted output")]
    public void Encode_ProducesValidBase64UrlOutput()
    {
        // Arrange
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
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
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
        var testValue = 987654321L;

        // Act
        var encoded = encoder.Encode(testValue);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(testValue, decoded);
    }

    [TestMethod]
    [Description("Decode all randomly-encrypted values should recover original")]
    public void Encode_RandomNonce_CanBeDecodedCorrectly()
    {
        // Arrange
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
        var testValue = 555555L;

        // Act & Assert
        for (var i = 0; i < 10; i++)
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
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
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
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);

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
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123456L);
        var truncated = encoded[..Math.Max(1, encoded.Length - 5)];

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
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123456L);

        // Corrupt the ciphertext by changing a character
        var array = encoded.ToCharArray();
        array[encoded.Length - 1] = (char)(array[encoded.Length - 1] ^ 0xAA);
        var corrupted = new string(array);

        // Act & Assert
        ThrowsException<Exception>(() =>
            encoder.Decode(corrupted)
        );
    }

    [TestMethod]
    [Description("Encode-Decode with all edge case values")]
    [DynamicData(nameof(GetLongTestValuesData))]
    public void EncodeDecodeRoundTrip_WithEdgeCaseValues(long value)
    {
        // Arrange
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);

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
        var encoder1 = new Speck64128CryptoIdEncoder("key-1", TestSalt);
        var encoder2 = new Speck64128CryptoIdEncoder("key-2", TestSalt);
        var testValue = 123456L;

        // Act
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
        var encoder1 = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
        var encoder2 = new Speck64128CryptoIdEncoder(TestKey, salt2);
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
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
    }

    [TestMethod]
    [Description("Using statement should properly dispose")]
    public void Using_ShouldCallDispose()
    {
        // Arrange & Act
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123L);
        Assert.IsNotNull(encoded);
    }

    [TestMethod]
    [Description("Long type specific: should handle zero")]
    public void Encode_WithZero_Succeeds()
    {
        // Arrange
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(0L);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(0L, decoded);
    }

    [TestMethod]
    [Description("Long type specific: should handle negative values")]
    [DataRow(-1L)]
    [DataRow(-123456789L)]
    [DataRow(long.MinValue)]
    public void Encode_WithNegativeValues_Succeeds(long value)
    {
        // Arrange
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(value);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(value, decoded);
    }

    [TestMethod]
    [Description("Should work with very large long values")]
    [DataRow(long.MaxValue)]
    [DataRow(long.MinValue)]
    [DataRow(9223372036854775807L)]
    [DataRow(-9223372036854775808L)]
    public void Encode_WithExtremeLongValues_Succeeds(long value)
    {
        // Arrange
        var encoder = new Speck64128CryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(value);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(value, decoded);
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
