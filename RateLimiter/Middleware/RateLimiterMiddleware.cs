using System;
using System.Threading.Tasks;
using RateLimiter.Algorithms;
using RateLimiter.Domain;
using RateLimiter.Storage;

namespace RateLimiter.Middleware;
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
        ArgumentNullException.ThrowIfNull(rulesStorage);
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(configuration);

        _rulesStorage = rulesStorage;
        _algorithm = algorithm;
        _configuration = configuration;
    }

    public async Task<RateLimitResult> HandleRequestAsync(RateLimiterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

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
