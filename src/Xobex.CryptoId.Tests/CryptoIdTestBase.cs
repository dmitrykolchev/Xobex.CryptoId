using System.Text;
using Xobex.Cryptography;
using Xobex.Cryptography.Abstractions;

namespace Xobex.CryptoId.Tests;

/// <summary>
/// Base class for CryptoId tests providing shared test data and utilities.
/// </summary>
[TestClass]
public abstract class CryptoIdTestBase
{
    /// <summary>
    /// Standard test key used across all tests for consistency.
    /// </summary>
    protected const string TestKey = "my-secret-key-for-testing";

    /// <summary>
    /// Standard test salt used across all tests.
    /// </summary>
    protected static readonly byte[] TestSalt = Convert.FromHexString("6b4e3a9f1c8d2e7b0a5f4c3d9e1b8a2f");

    /// <summary>
    /// Gets commonly used test values for edge case testing.
    /// </summary>
    /// <returns>Collection of test data tuples.</returns>
    protected static IEnumerable<long> GetLongTestValues()
    {
        yield return 0L;
        yield return 1L;
        yield return -1L;
        yield return 123456789L;
        yield return -123456789L;
        yield return long.MaxValue;
        yield return long.MinValue;
    }

    /// <summary>
    /// Gets commonly used test values for int edge case testing.
    /// </summary>
    /// <returns>Collection of test data tuples.</returns>
    protected static IEnumerable<int> GetIntTestValues()
    {
        yield return 0;
        yield return 1;
        yield return -1;
        yield return 123456;
        yield return -123456;
        yield return int.MaxValue;
        yield return int.MinValue;
    }

    /// <summary>
    /// Verifies that a Base64Url encoded string is valid.
    /// </summary>
    /// <param name="encoded">The encoded string to verify.</param>
    /// <returns>True if valid Base64Url format.</returns>
    protected static bool IsValidBase64Url(string encoded)
    {
        // Base64Url characters: A-Z, a-z, 0-9, -, _
        // Should not contain: +, /, = (padding)
        return !string.IsNullOrEmpty(encoded) &&
               encoded.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    /// <summary>
    /// Helper method to assert that an action throws an exception of type T.
    /// </summary>
    /// <typeparam name="T">The expected exception type.</typeparam>
    /// <param name="action">The action that should throw.</param>
    /// <returns>The thrown exception.</returns>
    protected static T ThrowsException<T>(Action action) where T : Exception
    {
        try
        {
            action();
            Assert.Fail($"Expected exception of type {typeof(T).Name} but no exception was thrown.");
            return null!; // Unreachable
        }
        catch (T ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected exception of type {typeof(T).Name} but got {ex.GetType().Name}: {ex.Message}");
            return null!; // Unreachable
        }
    }
}
