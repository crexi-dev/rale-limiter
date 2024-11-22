using NUnit.Framework;
using RateLimiter.Base;
using RateLimiter.Config;
using System.Threading;

namespace RateLimiter.Tests;

[TestFixture]
public class FixedWindowLimiterTests
{
    private FixedWindowLimiter _fixedWindowLimiter;
    private LimiterConfig _limiterConfig;

    [SetUp]
    public void SetUp()
    {
        _limiterConfig = new LimiterConfig
        {
            LimiterType = LimiterType.FixedWindowLimiter,
            MaxTokens = 10,
            MaxTimeInSeconds = 10
        };
        _fixedWindowLimiter = new FixedWindowLimiter(_limiterConfig);
    }

    [Test]
    public void AcquireLease_ShouldReturnAcquiredLease_WhenTokensAreAvailable()
    {
        // Arrange
        var leaseConfig = new LeaseConfig { ResourceName = "testResource" };

        // Act
        var lease = _fixedWindowLimiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsTrue(lease.IsAcquired);
        Assert.AreEqual("testResource", lease.ResourceName);
        Assert.AreEqual(1, _fixedWindowLimiter.CurrentTokens);
    }

    [Test]
    public void AcquireLease_ShouldReturnNonAcquiredLease_WhenTokensAreNotAvailable()
    {
        // Arrange
        var leaseConfig = new LeaseConfig { ResourceName = "testResource" };

        for (int i = 0; i < _limiterConfig.MaxTokens; i++)
        {
            _fixedWindowLimiter.AcquireLease(leaseConfig);
        }

        // Act
        var lease = _fixedWindowLimiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsFalse(lease.IsAcquired);
        Assert.AreEqual("testResource", lease.ResourceName);
        Assert.AreEqual(_limiterConfig.MaxTokens, _fixedWindowLimiter.CurrentTokens);
    }

    [Test]
    public void AcquireLease_ShouldResetTokens_WhenTimeWindowHasPassed()
    {
        // Arrange
        var limiterConfig = new LimiterConfig
        {
            LimiterType = LimiterType.FixedWindowLimiter,
            MaxTokens = 10,
            MaxTimeInSeconds = 1
        };
        var fixedWindowLimiter = new FixedWindowLimiter(limiterConfig);
        var leaseConfig = new LeaseConfig { ResourceName = "testResource" };

        fixedWindowLimiter.AcquireLease(leaseConfig);
        // Wait for 1 second.
        Thread.Sleep(1100);

        // Act
        var lease = fixedWindowLimiter.AcquireLease(leaseConfig);

        // Assert
        Assert.IsTrue(lease.IsAcquired);
        Assert.AreEqual(1, fixedWindowLimiter.CurrentTokens);
    }

    [Test]
    public void ReleaseLease_ShouldRemoveLeaseFromLeasesList()
    {
        // Arrange
        var leaseConfig = new LeaseConfig { ResourceName = "testResource" };
        var lease = _fixedWindowLimiter.AcquireLease(leaseConfig);

        // Act
        lease.RelinquishLease();

        // Assert
        Assert.IsTrue(lease.IsAcquired == false);
        Assert.IsTrue(lease.IsRelinquished == true);
    }
}

