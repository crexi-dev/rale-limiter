using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter;
public class RateLimiter
{
    private readonly Dictionary<string, IRateLimitingRule> _resourceRules;

    public RateLimiter(Dictionary<string, IRateLimitingRule> resourceRules)
    {
        _resourceRules = resourceRules;
    }

    public bool IsRequestAllowed(string resource, string accessToken, DateTime requestTime)
    {
        if (!_resourceRules.ContainsKey(resource))
        {
            throw new ArgumentException("Resource not configured for rate limiting.");
        }

        return _resourceRules[resource].IsRequestAllowed(accessToken, requestTime);
    }
}