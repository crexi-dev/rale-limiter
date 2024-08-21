using RateLimiter.Interfaces;
using RateLimiter.Rules;
using RateLimiter.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules;

public class CompositeRateLimitingRuleTests : IDisposable
{
    private readonly IDatabase _redisDatabase;
    private readonly string _accessToken;
    private DateTime _requestTime;

    public CompositeRateLimitingRuleTests()
    {
        _redisDatabase = RedisConnectionManager.GetDatabase();
        _accessToken = "comp-test-token";
        _requestTime = DateTime.Now;
    }

    [Fact]
    public void Should_AllowRequest_WhenAllRulesPass()
    {
        var timeWindowRule = new TimeWindowRateLimitingRule(5, TimeSpan.FromMinutes(1), _redisDatabase);
        var intervalRule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);
        var compositeRule = new CompositeRateLimitingRule(new List<IRateLimitingRule> { timeWindowRule, intervalRule });

        // First request should pass
        Assert.True(compositeRule.IsRequestAllowed(_accessToken, _requestTime));

        // Wait for the interval + 10 seconds
        _requestTime = _requestTime.AddSeconds(10);
        Assert.True(compositeRule.IsRequestAllowed(_accessToken, _requestTime));
    }

    [Fact]
    public void Should_DenyRequest_WhenAnyRuleFails()
    {
        var timeWindowRule = new TimeWindowRateLimitingRule(1, TimeSpan.FromMinutes(1), _redisDatabase);
        var intervalRule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);
        var compositeRule = new CompositeRateLimitingRule(new List<IRateLimitingRule> { timeWindowRule, intervalRule });

        // First request should pass
        Assert.True(compositeRule.IsRequestAllowed(_accessToken, _requestTime));

        // Second request should be denied
        Assert.False(compositeRule.IsRequestAllowed(_accessToken, _requestTime));

        // Interval has passed => time window rule denied
        _requestTime = _requestTime.AddSeconds(10);
        Assert.False(compositeRule.IsRequestAllowed(_accessToken, _requestTime));
    }

    public void Dispose()
    {
        var timeWindowKey = $"rate_limit:time_window:{_accessToken}";
        var intervalKey = $"rate_limit:interval:{_accessToken}";
        _redisDatabase.KeyDelete(timeWindowKey);
        _redisDatabase.KeyDelete(intervalKey);
    }
}
