using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Rules;

namespace RateLimiter.Storage;

public interface IRateLimiterStorage<TKey>
{
    ValueTask AddRateLimitRuleAsync(TKey key, IRateLimitRule rateLimitRule, CancellationToken ct = default);
    ValueTask<IRateLimitRule> GetRuleAsync(TKey key, CancellationToken ct = default);
}
