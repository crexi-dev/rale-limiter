using NUnit.Framework;
using RateLimiter.Base;
using RateLimiter.Config;
using System;

namespace RateLimiter.Tests;

[TestFixture]
public class TokenLimiterTests
{
    private TokenLimiter _tokenLimiter;
    private LimiterConfig _limiterConfig;

    [SetUp]
    public void SetUp()
    {
        _limiterConfig = new LimiterConfig
        {
            LimiterType = LimiterType.TokenLimiter,
            MaxTokens = 10
        };
        _tokenLimiter = new TokenLimiter(_limiterConfig);
    }

    [Test]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(_limiterConfig.MaxTokens, _tokenLimiter.MaxTokens);
        Assert.AreEqual(_limiterConfig.MaxTokens, _tokenLimiter.AvailableTokens);
    }

    [Test]
    public void AcquireLease_ShouldAcquireLease_WhenTokensAreAvailable()
    {
        // Arrange
        var leaseConfig = new LeaseConfig
        {
            ResourceName = "Resource1",
            Tokens = 5
        };

        // Act
        var lease = _tokenLimiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsTrue(lease.IsAcquired);
        Assert.AreEqual(leaseConfig.Tokens, lease.Tokens);
        Assert.AreEqual(_tokenLimiter.AvailableTokens, _limiterConfig.MaxTokens - leaseConfig.Tokens);
    }

    [Test]
    public void AcquireLease_ShouldNotAcquireLease_WhenTokensAreNotAvailable()
    {
        // Arrange
        var leaseConfig = new LeaseConfig
        {
            ResourceName = "Resource1",
            Tokens = 15
        };

        // Act
        var lease = _tokenLimiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsFalse(lease.IsAcquired);
        Assert.AreEqual(0, lease.Tokens);
        Assert.AreEqual(_tokenLimiter.AvailableTokens, _limiterConfig.MaxTokens);
    }

    [Test]
    public void AcquireLease_ShouldThrowException_WhenLeaseConfigIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => _tokenLimiter.AcquireLease(null));
    }

    [Test]
    public void AcquireLease_ShouldThrowException_WhenTokensAreNull()
    {
        // Arrange
        var leaseConfig = new LeaseConfig
        {
            ResourceName = "Resource1",
            Tokens = null
        };

        // Assert
        Assert.Throws<ArgumentNullException>(() => _tokenLimiter.AcquireLease(leaseConfig));
    }

    [Test]
    public void AcquireLease_ShouldThrowException_WhenTokensAreLessThanOne()
    {
        // Arrange
        var leaseConfig = new LeaseConfig
        {
            ResourceName = "Resource1",
            Tokens = 0
        };

        // Assert
        Assert.Throws<ArgumentException>(() => _tokenLimiter.AcquireLease(leaseConfig));
    }

    [Test]
    public void AcquireLease_ShouldNotAcquireLease_WhenTokensExceedMaxTokens()
    {
        // Arrange
        var leaseConfig = new LeaseConfig
        {
            ResourceName = "Resource1",
            Tokens = 20
        };

        // Act
        var lease = _tokenLimiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsFalse(lease.IsAcquired);
        Assert.AreEqual(0, lease.Tokens);
        Assert.AreEqual(_tokenLimiter.AvailableTokens, _limiterConfig.MaxTokens);
    }

    [Test]
    public void ReleaseLease_ShouldReleaseLease_WhenLeaseIsValid()
    {
        // Arrange
        var leaseConfig = new LeaseConfig
        {
            ResourceName = "Resource1",
            Tokens = 5
        };

        var lease = _tokenLimiter.AcquireLease(leaseConfig);

        // Act
        lease.RelinquishLease();

        // Assert
        Assert.AreEqual(_tokenLimiter.AvailableTokens, _limiterConfig.MaxTokens);
    }
}

