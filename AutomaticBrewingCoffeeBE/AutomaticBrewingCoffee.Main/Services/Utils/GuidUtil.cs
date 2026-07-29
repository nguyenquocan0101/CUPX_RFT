using System.Security.Cryptography;
using System.Text;

namespace Services.Utils;

public class GuidUtil
{
    public static string ShortenGuid(Guid guid)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(guid.ToString()));
            return BitConverter.ToString(hash).Replace("-", "").Substring(0, 4);
        }
    }
}