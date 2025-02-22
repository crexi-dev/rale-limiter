using RateLimiter.Models;
using RateLimiter.Rules;
using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IRateLimiter
    {
        void RegisterRule(string resourceId, IRateLimitRule rule);
        Task<bool> IsRequestAllowedAsync(RequestModel request);
    }
}
