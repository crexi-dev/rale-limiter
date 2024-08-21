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

public class GeographicalRateLimitingRuleTests : IDisposable
{
    private readonly IDatabase _redisDatabase;
    private readonly string _usAccessToken;
    private readonly string _euAccessToken;
    private DateTime _requestTime;

    public GeographicalRateLimitingRuleTests()
    {
        _redisDatabase = RedisConnectionManager.GetDatabase();
        _usAccessToken = "US-test-token";
        _euAccessToken = "EU-test-token";
        _requestTime = DateTime.Now;
    }

    [Fact]
    public void Should_ApplyUSRule_WhenUSTokenProvided()
    {
        var usRule = new TimeWindowRateLimitingRule(5, TimeSpan.FromMinutes(1), _redisDatabase);
        var euRule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);
        var regionRules = new Dictionary<string, IRateLimitingRule>
            {
                { "US", usRule },
                { "EU", euRule }
            };
        var geoRule = new GeographicalRateLimitingRule(regionRules);

        // Test US rule
        for (int i = 0; i < 5; i++)
        {
            Assert.True(geoRule.IsRequestAllowed(_usAccessToken, _requestTime));
        }

        // 6th request should be denied
        Assert.False(geoRule.IsRequestAllowed(_usAccessToken, _requestTime));
    }

    [Fact]
    public void Should_ApplyEURule_WhenEUTokenProvided()
    {
        var usRule = new TimeWindowRateLimitingRule(5, TimeSpan.FromMinutes(1), _redisDatabase);
        var euRule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);
        var regionRules = new Dictionary<string, IRateLimitingRule>
            {
                { "US", usRule },
                { "EU", euRule }
            };
        var geoRule = new GeographicalRateLimitingRule(regionRules);

        // First request should be passed
        Assert.True(geoRule.IsRequestAllowed(_euAccessToken, _requestTime));

        // Second request within the 10-second interval should be denied based on the EU rule
        _requestTime = _requestTime.AddSeconds(8);
        Assert.False(geoRule.IsRequestAllowed(_euAccessToken, _requestTime));
    }

    [Fact]
    public void Should_DenyRequest_WhenUnknownRegion()
    {
        var usRule = new TimeWindowRateLimitingRule(5, TimeSpan.FromMinutes(1), _redisDatabase);
        var euRule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);
        var regionRules = new Dictionary<string, IRateLimitingRule>
            {
                { "US", usRule },
                { "EU", euRule }
            };
        var geoRule = new GeographicalRateLimitingRule(regionRules);

        var unknownToken = "AS-test-token";
        Assert.False(geoRule.IsRequestAllowed(unknownToken, _requestTime));
    }

    public void Dispose()
    {
        var usTimeWindowKey = $"rate_limit:time_window:{_usAccessToken}";
        var euIntervalKey = $"rate_limit:interval:{_euAccessToken}";
        _redisDatabase.KeyDelete(usTimeWindowKey);
        _redisDatabase.KeyDelete(euIntervalKey);
    }
}
