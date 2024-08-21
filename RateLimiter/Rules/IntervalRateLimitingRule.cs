using RateLimiter.Interfaces;
using StackExchange.Redis;
using System;

namespace RateLimiter.Rules;

public class IntervalRateLimitingRule : IRateLimitingRule
{
    private readonly TimeSpan _minimumInterval;
    private readonly IDatabase _redisDatabase;

    public IntervalRateLimitingRule(TimeSpan minimumInterval, IDatabase redisDatabase)
    {
        _minimumInterval = minimumInterval;
        _redisDatabase = redisDatabase;
    }

    public bool IsRequestAllowed(string accessToken, DateTime requestTime)
    {
        string redisKey = $"rate_limit:interval:{accessToken}";
        var lastRequestTime = _redisDatabase.StringGet(redisKey);

        if (!lastRequestTime.HasValue)
        {
            _redisDatabase.StringSet(redisKey, requestTime.ToString("o"));
            return true;
        }

        var lastRequest = DateTime.Parse(lastRequestTime);
        if (requestTime - lastRequest < _minimumInterval)
        {
            return false;
        }

        _redisDatabase.StringSet(redisKey, requestTime.ToString("o"));
        return true;
    }
}
