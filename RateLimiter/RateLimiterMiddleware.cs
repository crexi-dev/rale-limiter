using System;
using System.Threading.Tasks;

namespace RateLimiter;
public class RateLimiterMiddleware
{
    private readonly IRateLimiterRuleStorage _rulesStorage;
    private readonly IRateLimitStateStorage<int> _ruleStateStorage;
    private readonly IRateLimitAlgorithm _algorithm;

    public RateLimiterMiddleware(
        IRateLimiterRuleStorage rulesStorage,
        IRateLimitStateStorage<int> ruleStateStorage,
        IRateLimitAlgorithm algorithm)
    {
        _rulesStorage = rulesStorage;
        _ruleStateStorage = ruleStateStorage;
        _algorithm = algorithm;
    }

    public async Task<RateLimitResult> HandleRequestAsync(RateLimiterRequest request)
    {
        var rule = await _rulesStorage.GetRuleAsync(request.Domain, request.Descriptor).ConfigureAwait(false);

        // If rule doesn't exist then let the request through.
        if (rule == null)
        {
            return new RateLimitResult(false, -1, TimeSpan.Zero);
        }

        // block requests associated with "blacklist" rules
        if (rule.RateLimit.MaxRequests == 0)
        {
            return new RateLimitResult(true, 0, TimeSpan.Zero);
        }

        var result = await _algorithm.HandleRequestAsync(rule).ConfigureAwait(false);

        return result;
    }
}
