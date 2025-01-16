using System;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter;
public class RateLimiterMiddleware
{
    private readonly IRateLimiterRuleStorage _rulesStorage;
    private readonly IRateLimitAlgorithm _algorithm;
    private readonly RateLimiterMiddlewareConfiguration _configuration;

    public RateLimiterMiddleware(
        IRateLimiterRuleStorage rulesStorage,
        IRateLimitAlgorithm algorithm,
        RateLimiterMiddlewareConfiguration configuration)
    {
        _rulesStorage = rulesStorage;
        _algorithm = algorithm;
        _configuration = configuration;
    }

    public async Task<RateLimitResult> HandleRequestAsync(RateLimiterRequest request)
    {
        var rule = await _rulesStorage.GetRuleAsync(request.Domain, request.Descriptor).ConfigureAwait(false);

        // If rule doesn't exist then let the request through.
        if (rule == null)
        {
            if (_configuration.NoAssociatedRuleBehaviorHandling == NoAssociatedRuleBehavior.AllowRequest)
            {
                return new RateLimitResult(false, -1, TimeSpan.Zero);
            }

            return new RateLimitResult(true, 0, TimeSpan.Zero);
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
