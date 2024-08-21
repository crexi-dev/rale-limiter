using RateLimiter.Rules;
using RateLimiter.Services;
using StackExchange.Redis;

namespace RateLimiter.Tests.Rules;

public class IntervalRateLimitingRuleTests : IDisposable
{
    private readonly IDatabase _redisDatabase;
    private readonly string _accessPassToken;
    private readonly string _accessDenyToken;
    private readonly DateTime _requestTime;

    public IntervalRateLimitingRuleTests()
    {
        _redisDatabase = RedisConnectionManager.GetDatabase();
        _accessPassToken = "interval-pass-test-token";
        _accessDenyToken = "interval-deny-test-token";
        _requestTime = DateTime.Now;
    }

    [Fact]
    public void Should_AllowRequest_WhenIntervalPassed()
    {
        var rule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);

        // First request should be passed
        Assert.True(rule.IsRequestAllowed(_accessPassToken, _requestTime));

        // Adding 10 seconds => Second request should be passed
        Assert.True(rule.IsRequestAllowed(_accessPassToken, _requestTime.AddSeconds(10)));
    }

    [Fact]
    public void Should_DenyRequest_WhenIntervalHasNotPassed()
    {
        var rule = new IntervalRateLimitingRule(TimeSpan.FromSeconds(10), _redisDatabase);

        // First request should be passed
        Assert.True(rule.IsRequestAllowed(_accessDenyToken, _requestTime));

        // Second request within 10 second -> deny
        Assert.False(rule.IsRequestAllowed(_accessDenyToken, _requestTime.AddSeconds(8)));
    }

    // Cleanup Redis
    public void Dispose()
    {
        var redisPassKey = $"rate_limit:interval:{_accessPassToken}";
        _redisDatabase.KeyDelete(redisPassKey);

        var redisDenyKey = $"rate_limit:interval:{_accessDenyToken}";
        _redisDatabase.KeyDelete(redisDenyKey);
    }
}
