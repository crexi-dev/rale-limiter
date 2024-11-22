using NUnit.Framework;
using RateLimiter.Base;
using RateLimiter.Config;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RateLimiter.Tests;

/// <summary>
/// This class demonstrates via tests the ability to initialize the rate limiter configuration from a JSON file.
/// The related JSON data has been stored in the file SampleResourceRateLimiterConfig.json.
/// </summary>
[TestFixture]
public class JsonInitializationTests
{
    private List<ResourceLimiterConfig> _resourceLimitConfig;

    [SetUp]
    public void SetUp()
    {
        string json = File.ReadAllText("../../../../SampleResourceRateLimiterConfig.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        JsonConfig config = JsonSerializer.Deserialize<JsonConfig>(json, options);
        _resourceLimitConfig = config.RateLimiterConfig;
    }

    [Test]
    public void Resource1_TokenLimiter_ConfigIsValid()
    {
        // Arrange
        var resourceLimiterConfig = _resourceLimitConfig[0];

        // Act & Assert
        Assert.AreEqual(1, resourceLimiterConfig.Resources.Count);
        Assert.AreEqual("Resource1", resourceLimiterConfig.Resources[0].ResourceName);

        Assert.AreEqual(1, resourceLimiterConfig.Limiters.Count);
        Assert.AreEqual(LimiterType.TokenLimiter, resourceLimiterConfig.Limiters[0].LimiterType);
        Assert.AreEqual(10, resourceLimiterConfig.Limiters[0].MaxTokens);
    }

    [Test]
    public void Resource2_FixedWindowLimiter_ConfigIsValid()
    {
        // Arrange
        var resourceLimiterConfig = _resourceLimitConfig[1];

        // Act & Assert
        Assert.AreEqual(1, resourceLimiterConfig.Resources.Count);
        Assert.AreEqual("Resource2", resourceLimiterConfig.Resources[0].ResourceName);

        Assert.AreEqual(1, resourceLimiterConfig.Limiters.Count);
        Assert.AreEqual(LimiterType.FixedWindowLimiter, resourceLimiterConfig.Limiters[0].LimiterType);
        Assert.AreEqual(20, resourceLimiterConfig.Limiters[0].MaxTokens);
        Assert.AreEqual(10, resourceLimiterConfig.Limiters[0].MaxTimeInSeconds);
    }

    [Test]
    public void Resource3And4_TokenAndFixedWindowLimiter_ConfigIsValid()
    {
        // Arrange
        var resourceLimiterConfig = _resourceLimitConfig[2];

        // Act & Assert
        Assert.AreEqual(2, resourceLimiterConfig.Resources.Count);
        Assert.AreEqual("Resource3", resourceLimiterConfig.Resources[0].ResourceName);
        Assert.AreEqual("Resource4", resourceLimiterConfig.Resources[1].ResourceName);

        Assert.AreEqual(2, resourceLimiterConfig.Limiters.Count);
        Assert.AreEqual(LimiterType.TokenLimiter, resourceLimiterConfig.Limiters[0].LimiterType);
        Assert.AreEqual(10, resourceLimiterConfig.Limiters[0].MaxTokens);

        Assert.AreEqual(LimiterType.FixedWindowLimiter, resourceLimiterConfig.Limiters[1].LimiterType);
        Assert.AreEqual(5, resourceLimiterConfig.Limiters[1].MaxTokens);
        Assert.AreEqual(10, resourceLimiterConfig.Limiters[1].MaxTimeInSeconds);
    }

    [Test]
    public void AcquireLease_Resource1_TokenLimiter()
    {
        // Arrange
        var resourceLimiterConfig = _resourceLimitConfig[0];
        var leaseConfig = resourceLimiterConfig.Resources[0];
        var limiterConfig = resourceLimiterConfig.Limiters[0];

        // Act
        var limiter = LimiterFactory.CreateRateLimiter(limiterConfig);
        var lease = limiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsTrue(limiter.LimiterType == LimiterType.TokenLimiter);
        Assert.IsTrue(lease.IsAcquired);
        Assert.AreEqual("Resource1", lease.ResourceName);
    }

    [Test]
    public void AcquireLease_Resource2_FixedWindowLimiter()
    {
        // Arrange
        var resourceLimiterConfig = _resourceLimitConfig[1];
        var leaseConfig = resourceLimiterConfig.Resources[0];
        var limiterConfig = resourceLimiterConfig.Limiters[0];

        // Act
        var limiter = LimiterFactory.CreateRateLimiter(limiterConfig);
        var lease = limiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsTrue(limiter.LimiterType == LimiterType.FixedWindowLimiter);
        Assert.IsTrue(lease.IsAcquired);
        Assert.AreEqual("Resource2", lease.ResourceName);
    }

    [Test]
    public void AcquireLease_Resource3And4_TokenAndFixedWindowLimiter()
    {
        // Arrange
        var resourceLimiterConfig = _resourceLimitConfig[2];
        var leaseConfig1 = resourceLimiterConfig.Resources[0];
        var leaseConfig2 = resourceLimiterConfig.Resources[1];
        var limiterConfig1 = resourceLimiterConfig.Limiters[0];
        var limiterConfig2 = resourceLimiterConfig.Limiters[1];

        // Act
        var limiter = LimiterFactory.CreateRateLimiter(resourceLimiterConfig.Limiters);
        var lease1 = limiter.AcquireLease(leaseConfig1);
        var lease2 = limiter.AcquireLease(leaseConfig2);

        // Assert
        Assert.IsTrue(limiter.LimiterType == LimiterType.LinkedLimiter);

        Assert.IsTrue(lease1.IsAcquired);
        Assert.AreEqual("Resource3", lease1.ResourceName);

        Assert.IsTrue(lease2.IsAcquired);
        Assert.AreEqual("Resource4", lease2.ResourceName);
    }
}

