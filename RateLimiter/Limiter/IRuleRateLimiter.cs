using System.Threading;
using RateLimiter.Models;
using System.Threading.Tasks;

namespace RateLimiter.Limiter;

public interface IRuleRateLimiter<TResource, TKey>
{
    ValueTask<RuleResult> ApplyRateLimitRulesAsync(TResource resource, CancellationToken ct = default);
}
