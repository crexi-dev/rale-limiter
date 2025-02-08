using RateLimiter.Abstractions;
using RateLimiter.Config;

using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

public class RateLimiter : IRateLimitRequests
{
    private readonly IEnumerable<IRateLimitRule> _rules;

    public RateLimiter(
        IProvideRateLimitRules rulesFactory)
    {
        _rules = rulesFactory.GetRules();
    }

    public (bool, string) IsRequestAllowed(IEnumerable<RateLimited> rules)
    {
        var passed = _rules.All(rule => rule.IsAllowed("foo"));

        return passed ? (passed, string.Empty) :
            (passed, "some message about banging on our door too much");
    }
}