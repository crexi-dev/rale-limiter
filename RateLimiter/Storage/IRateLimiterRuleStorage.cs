using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter.Storage;
public interface IRateLimiterRuleStorage
{
    Task AddOrUpdateRuleAsync(RateLimitRule rule, CancellationToken token = default);
    Task<RateLimitRule?> GetRuleAsync(
        string domain,
        RateLimitDescriptor descriptor,
        CancellationToken token = default);
}
