using RateLimiter.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;


namespace RateLimiter.Rules;

public class GeographicalRateLimitingRule : IRateLimitingRule
{
    private readonly IDictionary<string, IRateLimitingRule> _regionRules;

    public GeographicalRateLimitingRule(IDictionary<string, IRateLimitingRule> regionRules)
    {
        _regionRules = regionRules ?? throw new ArgumentNullException(nameof(regionRules));
    }

    public bool IsRequestAllowed(string accessToken, DateTime requestTime)
    {
        var regionCode = accessToken.Substring(0, 2).ToUpper();

        if (_regionRules.ContainsKey(regionCode))
        {
            return _regionRules[regionCode].IsRequestAllowed(accessToken, requestTime);
        }

        return false;
    }
}
