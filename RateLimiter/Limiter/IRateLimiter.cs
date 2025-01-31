using System.Threading;
using RateLimiter.Models;
using System.Threading.Tasks;

namespace RateLimiter.Limiter;

public interface IRateLimiter<TResource>
{
    ValueTask<RateLimitResult> ApplyRateLimitRulesAsync(TResource resource, CancellationToken ct = default);
}