using System;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Domain;
using RateLimiter.Storage;

namespace RateLimiter.Algorithms;

public class FixedWindowAlgorithm : IRateLimitAlgorithm
{
    private readonly IRateLimitStateStorage<int> _stateStorage;

    public FixedWindowAlgorithm(IRateLimitStateStorage<int> stateStorage)
    {
        _stateStorage = stateStorage;
    }

    public async Task<RateLimitResult> HandleRequestAsync(RateLimitRule rule, CancellationToken token = default)
    {
        var stateKey = rule.GetHashCode();

        // get the rule's state or a new state if this is the first execution of the rule
        var state = await _stateStorage.GetStateAsync(stateKey, token) ??
                    new RateLimitRuleState(0, GetCurrentTime());

        var currentTime = GetCurrentTime();

        // the request is made after the window duration expires so this is the first execution
        if (currentTime - state.WindowStart > rule.RateLimit.WindowDuration)
        {
            await _stateStorage.AddOrUpdateStateAsync(stateKey, new RateLimitRuleState(1, currentTime), token);

            return new RateLimitResult(
                false,
                rule.RateLimit.MaxRequests - 1,
                TimeSpan.Zero);
        }

        // the request is within the current window
        if (state.RequestsMade < rule.RateLimit.MaxRequests)
        {
            var requestsMade = state.RequestsMade + 1;

            await _stateStorage.AddOrUpdateStateAsync(stateKey,
                new RateLimitRuleState(requestsMade,
                    state.WindowStart),
                token);

            return new RateLimitResult(
                false,
                rule.RateLimit.MaxRequests - requestsMade,
                TimeSpan.Zero);
        }

        // the request is rate limited
        var retryAfter = TimeSpan.FromSeconds(Math.Ceiling((rule.RateLimit.WindowDuration - (currentTime - state.WindowStart)).TotalSeconds));

        return new RateLimitResult(
            true,
            0,
            retryAfter);
    }

    private static DateTime GetCurrentTime() => DateTime.UtcNow;
}
