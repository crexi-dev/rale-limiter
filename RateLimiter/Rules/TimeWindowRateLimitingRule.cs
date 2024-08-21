using RateLimiter.Interfaces;
using StackExchange.Redis;
using System;

namespace RateLimiter.Rules;

public class TimeWindowRateLimitingRule : IRateLimitingRule
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    private readonly IDatabase _redisDatabase;

    public TimeWindowRateLimitingRule(int maxRequests, TimeSpan windowSize, IDatabase redisDatabase)
    {
        _maxRequests = maxRequests;
        _windowSize = windowSize;
        _redisDatabase = redisDatabase;
    }

    public bool IsRequestAllowed(string accessToken, DateTime requestTime)
    {
        string redisKey = $"rate_limit:time_window:{accessToken}";
        var requestCount = _redisDatabase.StringIncrement(redisKey);

        if (requestCount == 1)
        {
            _redisDatabase.KeyExpire(redisKey, _windowSize);
        }

        return requestCount <= _maxRequests;
    }
}
