namespace Services.Utils;

/// <summary>
/// Provides utility methods for hashing and verifying passwords using the BCrypt algorithm.
/// </summary>
public static class Hasher
{
    /// <summary>
    /// Hashes the specified plain text value using the BCrypt algorithm.
    /// </summary>
    /// <param name="value">The plain text value to hash.</param>
    /// <returns>A hashed password string containing the salt and hash.</returns>
    public static string Hash(string value)
    {
        return BCrypt.Net.BCrypt.HashPassword(value);
    }

    /// <summary>
    /// Verifies that the specified plain text password matches the hashed password.
    /// </summary>
    /// <param name="value">The plain text value to verify.</param>
    /// <param name="hashedValue">The previously hashed value to compare against.</param>
    /// <returns><c>true</c> if the value matches the hash; otherwise, <c>false</c>.</returns>
    public static bool Verify(string value, string hashedValue)
    {
        return BCrypt.Net.BCrypt.Verify(value, hashedValue);
    }
}