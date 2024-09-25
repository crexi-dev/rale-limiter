using System.Security.Cryptography;
using System.Text;

namespace RateLimiter;

public class HashGenerator
{
    public static string GenerateHash(params string[] values)
    {
        var combinedString = string.Join("|", values);

        var combinedBytes = Encoding.UTF8.GetBytes(combinedString);

        var hashBytes = SHA256.HashData(combinedBytes);

        var hashStringBuilder = new StringBuilder();
        foreach (var b in hashBytes) hashStringBuilder.Append(b.ToString("x2"));

        return hashStringBuilder.ToString();
    }
}
