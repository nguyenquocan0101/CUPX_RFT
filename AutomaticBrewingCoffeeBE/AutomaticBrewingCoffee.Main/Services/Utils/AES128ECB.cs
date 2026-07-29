using System.Security.Cryptography;
using System.Text;

namespace Services.Utils;
public static class AES128ECB
{
    public static string EncryptAES128ECB(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(encrypted);
            }
        }
    }

    public static string DecryptAES128ECB(string base64CipherText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] cipherBytes = Convert.FromBase64String(base64CipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(decrypted);
            }
        }
    }
}

