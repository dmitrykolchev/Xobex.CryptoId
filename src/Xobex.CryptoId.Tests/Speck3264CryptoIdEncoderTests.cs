// <copyright file="Speck3264CryptoIdEncoderTests.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Cryptography;

namespace Xobex.CryptoId.Tests;

/// <summary>
/// Tests for Speck3264CryptoIdEncoder using Speck32/64 encryption for int IDs.
/// </summary>
[TestClass]
public class Speck3264CryptoIdEncoderTests : CryptoIdTestBase
{
    [TestMethod]
    [Description("Constructor should accept valid key and salt")]
    public void Constructor_WithValidInputs_InitializesSuccessfully()
    {
        // Arrange & Act
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);

        // Assert - No exception thrown
        Assert.IsNotNull(encoder);
    }

    [TestMethod]
    [Description("Constructor should throw when key is null")]
    public void Constructor_WithNullKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            new Speck3264CryptoIdEncoder(null!, TestSalt)
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
            new Speck3264CryptoIdEncoder("", TestSalt)
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
            new Speck3264CryptoIdEncoder(TestKey, null!)
        );
    }

    [TestMethod]
    [Description("Encode should produce Base64Url formatted output")]
    public void Encode_ProducesValidBase64UrlOutput()
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var testValue = 123456789;

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
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var testValue = 987654321;

        // Act
        var encoded = encoder.Encode(testValue);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(testValue, decoded);

        var result = encoder.TryDecode(encoded, out decoded);
        Assert.IsTrue(result, "TryDecode should succeed");
        Assert.AreEqual(testValue, decoded);
    }

    [TestMethod]
    [Description("TryEncode and Decode should round-trip successfully")]
    public void TryEncodeDecodeRoundTrip_ShouldRecoverOriginalValue()
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var testValue = 987654321;

        // Act
        var encodedBuffer = new char[128];
        var encoded = encoder.TryEncode(testValue, encodedBuffer, out var written);
        Assert.IsTrue(encoded, "TryEncode should succeed");

        var decoded = encoder.Decode(encodedBuffer.AsSpan(0, written));
        // Assert
        Assert.AreEqual(testValue, decoded);
    }

    [TestMethod]
    [Description("Decode all randomly-encrypted values should recover original")]
    public void Encode_RandomNonce_CanBeDecodedCorrectly()
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var testValue = 555555;

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
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var value1 = 111111;
        var value2 = 222222;

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
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);

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
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123456);
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
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123456);

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
    [DynamicData(nameof(GetIntTestValues))]
    public void EncodeDecodeRoundTrip_WithEdgeCaseValues(int value)
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);

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
        var encoder1 = new Speck3264CryptoIdEncoder("key-1", TestSalt);
        var encoder2 = new Speck3264CryptoIdEncoder("key-2", TestSalt);
        var testValue = 123456;

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
        var encoder1 = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var encoder2 = new Speck3264CryptoIdEncoder(TestKey, salt2);
        var testValue = 123456;

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
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
    }

    [TestMethod]
    [Description("Using statement should properly dispose")]
    public void Using_ShouldCallDispose()
    {
        // Arrange & Act
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);
        var encoded = encoder.Encode(123);
        Assert.IsNotNull(encoded);
    }

    [TestMethod]
    [Description("Long type specific: should handle zero")]
    public void Encode_WithZero_Succeeds()
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(0);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(0L, decoded);
    }

    [TestMethod]
    [Description("Long type specific: should handle negative values")]
    [DataRow(-1)]
    [DataRow(-123456789)]
    [DataRow(int.MinValue)]
    public void Encode_WithNegativeValues_Succeeds(int value)
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(value);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(value, decoded);
    }

    [TestMethod]
    [Description("Should work with very large long values")]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void Encode_WithExtremeLongValues_Succeeds(int value)
    {
        // Arrange
        var encoder = new Speck3264CryptoIdEncoder(TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(value);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(value, decoded);
    }

    [TestMethod]
    public void Speck32_64_MatchesOfficialTestVector()
    {
        // Официальный вектор (NSA paper, Beaulieu et al. 2013):
        // Key words (k0,k1,k2,k3) = (0x0100, 0x0908, 0x1110, 0x1918)
        // Plaintext (x,y) = (0x6574, 0x694c)
        // Ciphertext (x,y) = (0xa868, 0x42f2)
        // Каждое слово закодировано little-endian при сборке byte-массива —
        // порядок слов сохраняется, байты внутри слова свопаются.

        byte[] key =
        [
            0x00, 0x01, // k0 = 0x0100 LE
            0x08, 0x09, // k1 = 0x0908 LE
            0x10, 0x11, // k2 = 0x1110 LE
            0x18, 0x19, // k3 = 0x1918 LE
        ];

        byte[] plaintext = [0x74, 0x65, 0x4C, 0x69]; // (x=0x6574, y=0x694c), LE per word
        byte[] expectedCiphertext = [0x68, 0xA8, 0xF2, 0x42]; // (x=0xa868, y=0x42f2), LE per word

        var cipher = new Speck3264CryptoIdEncoder.Speck32_64(key);
        Span<byte> actual = stackalloc byte[4];
        cipher.Encrypt(plaintext, actual);

        Assert.IsTrue(actual.SequenceEqual(expectedCiphertext));
    }
}
