using RateLimiter.Storages;
using System.Threading.Tasks;

namespace RateLimiter.Rules;
public interface IRateLimitRule
{
    Task<bool> IsRequestAllowedAsync(string clientId, string actionKey, IRateLimitStore store);
}
