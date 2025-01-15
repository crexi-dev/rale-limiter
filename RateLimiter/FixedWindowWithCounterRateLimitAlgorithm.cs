using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public class FixedWindowWithCounterRateLimitAlgorithm : IRateLimitAlgorithm
{
    private readonly IRateLimiterStorage _storage;

    public FixedWindowWithCounterRateLimitAlgorithm(IRateLimiterStorage storage)
    {
        _storage = storage;
    }

    public async Task<RateLimitResult> ExecuteAsync(Rule rule, CancellationToken token = default)
    {
        var ruleState = await _storage.GetRuleStateAsync(rule.Scope, token);

        if (rule.Limit == 1 && ruleState.LastRequestTime.HasValue)
        {
            var elapsedTime = DateTime.UtcNow - ruleState.LastRequestTime.Value;

            if (elapsedTime < rule.Window)
            {
                return new RateLimitResult(true);
            }
        }
        else if (ruleState.RequestCount >= rule.Limit)
        {
            return new RateLimitResult(true);
        }

        ruleState.LastRequestTime = DateTime.UtcNow;
        await _storage.UpdateStateAsync(rule.Scope, ruleState, token);

        return new RateLimitResult();
    }
}
