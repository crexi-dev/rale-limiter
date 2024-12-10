using System.Threading.Tasks;

namespace RateLimiter.Domain;

public interface ITokenInspector
{
    Task<TokenInfo> GetInfo(string token);
}