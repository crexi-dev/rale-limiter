using RateLimiter.Rules;
using RateLimiter.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules;

public class TimeWindowRateLimitingRuleTests : IDisposable
{
    private readonly IDatabase _redisDatabase;
    private readonly string _accessToken;
    private readonly DateTime _requestTime;

    public TimeWindowRateLimitingRuleTests()
    {
        _redisDatabase = RedisConnectionManager.GetDatabase();
        _accessToken = "time-window-test-token";
        _requestTime = DateTime.Now;
    }

    [Fact]
    public void Should_AllowRequests_UntilLimitReahced()
    {
        var rule = new TimeWindowRateLimitingRule(5, TimeSpan.FromMinutes(1), _redisDatabase);

        for(int i = 0; i < 5; i++) {
            Assert.True(rule.IsRequestAllowed(_accessToken, _requestTime));
        }

        // 6th request should be denied
        Assert.False(rule.IsRequestAllowed(_accessToken, _requestTime));
    }

    public void Dispose()
    {
        var redisKey = $"rate_limit:time_window:{_accessToken}";
        _redisDatabase.KeyDelete(redisKey);
    }
}
