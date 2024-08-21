using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.Rules;

public class CompositeRateLimitingRule : IRateLimitingRule
{
    private readonly IList<IRateLimitingRule> _rules;

    public CompositeRateLimitingRule(IList<IRateLimitingRule> rules)
    {
        _rules = rules;
    }

    public bool IsRequestAllowed(string accessToken, DateTime requestTime)
    {
        foreach (var rule in _rules)
        {
            if (!rule.IsRequestAllowed(accessToken, requestTime))
            {
                return false;
            }
        }
        return true;
    }
}
