using RateLimiter.Models;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public interface IRateLimitRule
    {
        Task<bool> IsWithinLimitAsync(RequestModel request);
    }
}
