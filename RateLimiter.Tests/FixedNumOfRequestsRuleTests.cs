using NUnit.Framework;
using StackExchange.Redis;
using System;

namespace RateLimiter.Tests;

public class FixedNumOfRequestsRuleTests
{
    private IDatabase _database;
    private FixedNumOfRequestsRule _rule;
    private string _accessToken;

    [SetUp]
    public void Setup()
    {
        // Initialize Redis database
        _database = RedisHelper.Database;

        // Initialize the rule with max 5 requests per 60 seconds
        _rule = new FixedNumOfRequestsRule(5, 60,_database);

        // Example access token
        _accessToken = "test-token";

        // Clean up any existing data for the token
        Cleanup();
    }

    [TearDown]
    public void Cleanup()
    {
        // Clean up Redis
        _database.KeyDelete($"{_accessToken}:RequestCount");
        _database.KeyDelete($"{_accessToken}:LastRequestTime");
    }
    /// <summary>
    /// Should not rate limit (Result: false)
    /// </summary>
    [Test]
    public void ShouldRateLimit_FirstRequest()
    {
        // Arrange
        DateTime currentTime = DateTime.UtcNow;

        // Act
        bool result = _rule.ShouldRateLimit(_accessToken, currentTime);

        // Assert
        Assert.IsFalse(result);

        // Verify Redis values
        int currentRequestCount = (int)(_database.StringGet($"{_accessToken}:RequestCount") == RedisValue.Null ? 0 : _database.StringGet($"{_accessToken}:RequestCount"));
        string lastRequestTime = _database.StringGet($"{_accessToken}:LastRequestTime");


        Assert.AreEqual(1, currentRequestCount);
        Assert.AreEqual(currentTime.ToString(), lastRequestTime);
    }

    /// <summary>
    /// Should rate limit (Result: true)
    /// </summary>
    [Test]
    public void ShouldRateLimit_MaxRequestsReached()
    {
        // Arrange
        DateTime currentTime = DateTime.UtcNow;

        // Simulate reaching max requests
        _database.StringSet($"{_accessToken}:RequestCount", 5);
        _database.StringSet($"{_accessToken}:LastRequestTime", currentTime.ToString());

        // Act
        bool result = _rule.ShouldRateLimit(_accessToken, currentTime);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Should not rate limit (Result: False)
    /// </summary>
    [Test]
    public void ShouldRateLimit_TimeElapsedSinceLastRequest_ResetCountAndReturnsFalse()
    {
        // Arrange
        DateTime lastRequestTime = DateTime.UtcNow.AddSeconds(-61); // More than 60 seconds ago
        DateTime currentTime = DateTime.UtcNow;

        // Simulate max requests reached but time has elapsed
        _database.StringSet($"{_accessToken}:RequestCount", 5);
        _database.StringSet($"{_accessToken}:LastRequestTime", lastRequestTime.ToString());

        // Act
        bool result = _rule.ShouldRateLimit(_accessToken, currentTime);

        // Assert
        Assert.IsFalse(result);

        // Verify Redis values
        int currentRequestCount = (int)(_database.StringGet($"{_accessToken}:RequestCount") == RedisValue.Null ? 0 : _database.StringGet($"{_accessToken}:RequestCount"));
        string lastRequestTimeRedisVal = _database.StringGet($"{_accessToken}:LastRequestTime");

        Assert.AreEqual(1, currentRequestCount);
        Assert.AreEqual(currentTime.ToString(), lastRequestTimeRedisVal);

    }
}