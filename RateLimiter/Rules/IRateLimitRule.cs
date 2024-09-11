using System.Threading.Tasks;

namespace RateLimiter.Rules;

public interface IRateLimitRule
{
    Task<bool> IsRequestAllowed(string clientId, string resource);
}