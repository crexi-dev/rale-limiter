using System;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Models;
using RateLimiter.Storage;

namespace RateLimiter.Limiter;

public class RuleRateLimiter<TResource, TKey> : IRuleRateLimiter<TResource, TKey>
{
    private readonly string _ruleRateLimiterId = Guid.NewGuid().ToString();

    private readonly Func<TResource, RateLimitRuleByKeyFactory<TKey>> _ruleRateLimiter;
    private readonly IRateLimiterStorage<string> _rateLimiterStorage;

    public RuleRateLimiter(
        Func<TResource, RateLimitRuleByKeyFactory<TKey>> ruleRateLimiter,
        IRateLimiterStorage<string> rateLimiterStorage)
    {
        _ruleRateLimiter = ruleRateLimiter;
        _rateLimiterStorage = rateLimiterStorage;
    }

    public async ValueTask<RuleResult> ApplyRateLimitRulesAsync(TResource resource, CancellationToken ct = default)
    {
        var ruleRateLimiter = _ruleRateLimiter(resource);
        var key = $"{_ruleRateLimiterId}_{ruleRateLimiter.Key.GetHashCode()}";

        var rule = await _rateLimiterStorage.GetRuleAsync(key, ct);
        if (rule == null)
        {
            rule = ruleRateLimiter.LimitRule(ruleRateLimiter.Key);

            await _rateLimiterStorage.AddRateLimitRuleAsync(key, rule, ct);
        }

        var ruleResult = await rule.ApplyAsync(ct);

        return ruleResult;
    }
}
