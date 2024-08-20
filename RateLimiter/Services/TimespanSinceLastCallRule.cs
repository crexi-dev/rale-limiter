using System;
using System.Collections.Concurrent;
using RateLimiter.Contracts;

namespace RateLimiter.Services;

public class TimespanSinceLastCallRule(TimeSpan timeSpan) : IRateLimitRule
{
    private readonly ConcurrentDictionary<string, DateTime> lastRequestCache = new();
    public bool IsRequestAllowed(string clientToken)
    {
        var now = DateTime.UtcNow;
        if (!lastRequestCache.TryGetValue(clientToken, out var lastRequest) || (now - lastRequest) > timeSpan)
        {
            lastRequestCache.AddOrUpdate(clientToken, now, (s, time) => time);
            return true;
        }
        
        return false;
    }
}