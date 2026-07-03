using Xobex.Cryptography;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Tests;

/// <summary>
/// Tests for the CryptoIdFactory factory class and IdCiperAlgorithm enum.
/// </summary>
[TestClass]
public class CryptoIdEncoderTests : CryptoIdTestBase
{
    [TestMethod]
    [Description("Factory should create AES-GCM encoder for long type")]
    public void Create_WithAesGcmAndLongType_ReturnsAesCryptoIdEncoder()
    {
        // Arrange & Act
        var encoder = CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, TestKey, TestSalt);

        // Assert
        Assert.IsNotNull(encoder);
        Assert.IsInstanceOfType(encoder, typeof(AesGcmCryptoIdEncoder));
    }

    [TestMethod]
    [Description("Factory should create Speck64/128 encoder for long type")]
    public void Create_WithSpeck64128AndLongType_ReturnsSpeck64128CryptoIdEncoder()
    {
        // Arrange & Act
        var encoder = CryptoIdFactory.Create<long>(IdCipherAlgorithm.Speck64_128, TestKey, TestSalt);

        // Assert
        Assert.IsNotNull(encoder);
        Assert.IsInstanceOfType(encoder, typeof(Speck64128CryptoIdEncoder));
    }

    [TestMethod]
    [Description("Factory should create Speck32/64 encoder for int type")]
    public void Create_WithSpeck32_64AndIntType_ReturnsSpeck3264CryptoIdEncoder()
    {
        // Arrange & Act
        var encoder = CryptoIdFactory.Create<int>(IdCipherAlgorithm.Speck32_64, TestKey, TestSalt);

        // Assert
        Assert.IsNotNull(encoder);
        Assert.IsInstanceOfType(encoder, typeof(Speck3264CryptoIdEncoder));
    }

    [TestMethod]
    [Description("Factory should throw when unsupported algorithm for long type")]
    public void Create_WithSpeck32_64AndLongType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            CryptoIdFactory.Create<long>(IdCipherAlgorithm.Speck32_64, TestKey, TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("unsupported algorithm", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Factory should throw when unsupported algorithm for int type")]
    public void Create_WithSpeck64_128AndIntType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            CryptoIdFactory.Create<int>(IdCipherAlgorithm.Speck64_128, TestKey, TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("unsupported algorithm", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Factory should throw when AesGcm is used for int type")]
    public void Create_WithAesGcmAndIntType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            CryptoIdFactory.Create<int>(IdCipherAlgorithm.AesGcm, TestKey, TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("unsupported algorithm", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Factory should throw for unsupported types")]
    public void Create_WithUnsupportedType_ThrowsNotSupportedException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<NotSupportedException>(() =>
            CryptoIdFactory.Create<short>(IdCipherAlgorithm.AesGcm, TestKey, TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Factory should use default salt when null is provided")]
    public void Create_WithNullSalt_UsesDefaultSalt()
    {
        // Arrange & Act - Should not throw
        var encoder = CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, TestKey, null);

        // Assert
        Assert.IsNotNull(encoder);
    }

    [TestMethod]
    [Description("Factory should throw when key is null")]
    public void Create_WithNullKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, null!, TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Factory should throw when key is empty")]
    public void Create_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = ThrowsException<ArgumentException>(() =>
            CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, "", TestSalt)
        );
        Assert.IsTrue(ex.Message.Contains("null", StringComparison.OrdinalIgnoreCase) ||
                     ex.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [Description("Created encoders should be functional")]
    public void Create_EncodersAreUsable_CanEncodeAndDecode()
    {
        // Arrange
        var testValue = 123456789L;
        var encoder = CryptoIdFactory.Create<long>(IdCipherAlgorithm.AesGcm, TestKey, TestSalt);

        // Act
        var encoded = encoder.Encode(testValue);
        var decoded = encoder.Decode(encoded);

        // Assert
        Assert.AreEqual(testValue, decoded);
    }
}
