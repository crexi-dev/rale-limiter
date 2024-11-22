using NUnit.Framework;
using RateLimiter.Base;
using RateLimiter.Config;
using System.Collections.Generic;

namespace RateLimiter.Tests;

[TestFixture]
public class LimiterFactoryTests
{
    [Test]
    public void CreateRateLimiter_WithTokenLimiterConfig_ReturnsTokenLimiter()
    {
        // Arrange
        var config = new LimiterConfig
        {
            LimiterType = LimiterType.TokenLimiter,
            MaxTokens = 10
        };

        // Act
        var limiter = LimiterFactory.CreateRateLimiter(config);

        // Assert
        Assert.IsInstanceOf<TokenLimiter>(limiter);
        Assert.AreEqual(LimiterType.TokenLimiter, limiter.LimiterType);
    }

    [Test]
    public void CreateRateLimiter_WithFixedWindowLimiterConfig_ReturnsFixedWindowLimiter()
    {
        // Arrange
        var config = new LimiterConfig
        {
            LimiterType = LimiterType.FixedWindowLimiter,
            MaxTokens = 10,
            MaxTimeInSeconds = 60
        };

        // Act
        var limiter = LimiterFactory.CreateRateLimiter(config);

        // Assert
        Assert.IsInstanceOf<FixedWindowLimiter>(limiter);
        Assert.AreEqual(LimiterType.FixedWindowLimiter, limiter.LimiterType);
    }

    [Test]
    public void CreateRateLimiter_WithMultipleConfigs_ReturnsLinkedLimiter()
    {
        // Arrange
        var tokenLimiterConfig = new LimiterConfig
        {
            LimiterType = LimiterType.TokenLimiter,
            MaxTokens = 10
        };

        var fixedWindowLimiterConfig = new LimiterConfig
        {
            LimiterType = LimiterType.FixedWindowLimiter,
            MaxTokens = 10,
            MaxTimeInSeconds = 60
        };

        var configs = new List<LimiterConfig> { tokenLimiterConfig, fixedWindowLimiterConfig };

        // Act
        var limiter = LimiterFactory.CreateRateLimiter(configs);

        // Assert
        Assert.IsInstanceOf<LinkedLimiter>(limiter);
        Assert.AreEqual(LimiterType.LinkedLimiter, limiter.LimiterType);
    }
}