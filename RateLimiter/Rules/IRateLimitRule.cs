using System.Threading;
using RateLimiter.Models;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public interface IRateLimitRule
{
    ValueTask<RuleResult> ApplyAsync(CancellationToken ct = default);
}
