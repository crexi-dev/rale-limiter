using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter;
public interface IRateLimiterRuleStorage
{
    Task AddOrUpdateRuleAsync(RateLimitRule rule, CancellationToken token = default);
    Task<RateLimitRule?> GetRuleAsync(
        string domain,
        RateLimitDescriptor descriptor,
        CancellationToken token = default);
}
