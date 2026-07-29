using System.Security.Cryptography;
using System.Text;


namespace Services.Utils;

/// <summary>
/// Utility for secure random API key generation.
/// </summary>
public static class ApiKeyUtil
{
    // Default charset: 62 ký tự (A-Z, a-z, 0-9)
    private const string DefaultChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const string Key = "leminhducisagay!";

    /// <summary>
    /// Generates a secure, random API key.
    /// </summary>
    /// <param name="length">Number of characters in the API key. Recommended: >= 32</param>
    /// <param name="charset">
    /// Optional character set for the API key. Default: Alphanumeric (A-Z, a-z, 0-9)
    /// </param>
    /// <returns>Randomly generated API key string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if length is not positive or charset is empty.</exception>
    public static string GenerateApiKey(int length = 16, string charset = DefaultChars)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "API key length must be positive.");
        if (string.IsNullOrWhiteSpace(charset))
            throw new ArgumentException("Charset must not be empty.", nameof(charset));

        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        var keyBuilder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            keyBuilder.Append(charset[bytes[i] % charset.Length]);
        }

        return keyBuilder.ToString();
    }

    /// <summary>
    /// Encrypts plain text using AES-128 in ECB mode.
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (Key.Length != 16)
            throw new ArgumentException("AES key must be exactly 16 characters (128 bits).");

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] result = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                return Convert.ToBase64String(result);
            }
        }
    }

    /// <summary>
    /// Decrypts AES-128-ECB encrypted text.
    /// </summary>
    public static string Decrypt(string base64CipherText)
    {
        if (Key.Length != 16)
            throw new ArgumentException("AES key must be exactly 16 characters (128 bits).");

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                byte[] cipherBytes = Convert.FromBase64String(base64CipherText);
                byte[] result = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(result);
            }
        }
    }
}