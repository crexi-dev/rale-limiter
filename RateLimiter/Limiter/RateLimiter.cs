using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Models;

namespace RateLimiter.Limiter;

public class RateLimiter<TResource, TKey> : IRateLimiter<TResource>
{
    private readonly IList<RuleRateLimiter<TResource, TKey>> _ruleRateLimiters;

    public RateLimiter(IList<RuleRateLimiter<TResource, TKey>> ruleRateLimiters)
    {
        _ruleRateLimiters = ruleRateLimiters;
    }

    public async ValueTask<RateLimitResult> ApplyRateLimitRulesAsync(TResource resource, CancellationToken ct = default)
    {
        var ruleRateLimiterTasks = _ruleRateLimiters
            .Select(ruleLimiter => ruleLimiter.ApplyRateLimitRulesAsync(resource, ct).AsTask())
            .ToArray();

        var rulesResults = await Task.WhenAll(ruleRateLimiterTasks);
        var result = new RateLimitResult
        {
            IsAllowed = rulesResults.All(x => x.IsAllowed),
            RulesMessages = rulesResults.Select(x => x.RuleMessage).ToArray()
        };

        return result;
    }
}
