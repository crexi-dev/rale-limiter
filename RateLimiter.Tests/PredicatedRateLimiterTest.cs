using System;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class PredicatedRateLimiterTest
{
    [Test]
    public void CanCreatePredicatedRateLimiter()
    {
        var limiter = PredicatedRateLimiter.Create<int, int>(_ => true, _ => RateLimitPartition.GetNoLimiter(1));
        Assert.That(limiter, Is.Not.Null);
    }

    [Test]
    public void CanCreatePredicatedLimiterThatSkipsFailedPredicates()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedTimeSinceLimiter<int, int>(_ => false, 1,
            _ => new TimeSinceLimiterOptions { AllowedPeriod = TimeSpan.FromMinutes(10) });

        var lease = limiter.AttemptAcquire(1);
        Assert.That(lease.IsAcquired, Is.True);
        lease = limiter.AttemptAcquire(1);
        Assert.That(lease.IsAcquired, Is.True);
    }

    [Test]
    public void PredicatedLimiterLimitsPassedPredicates()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedTimeSinceLimiter<int, int>(_ => true, 1,
            _ => new TimeSinceLimiterOptions { AllowedPeriod = TimeSpan.FromMinutes(10) });

        var lease = limiter.AttemptAcquire(1);
        Assert.That(lease.IsAcquired, Is.True);
        lease = limiter.AttemptAcquire(1);

        Assert.That(lease.IsAcquired, Is.False);
    }

    [Test]
    public void DisposedLimiterThrows()
    {
        var limiter = PredicatedRateLimiter.Create<int, int>(_ => true, _ => RateLimitPartition.GetNoLimiter(1));
        limiter.Dispose();
        Assert.Throws<ObjectDisposedException>(() => limiter.AttemptAcquire(1));
    }

    [Test]
    public void PredicateChainLimitsOnProperLimiter()
    {
        var mockLease = new Mock<RateLimitLease>();
        mockLease.Setup(mock => mock.IsAcquired).Returns(true);
        var firstLimiterMock = new Mock<System.Threading.RateLimiting.RateLimiter>();
        firstLimiterMock.Protected().Setup<RateLimitLease>("AttemptAcquireCore", false, ItExpr.IsAny<int>())
            .Returns(mockLease.Object).Verifiable();
        var secondLimiterMock = new Mock<System.Threading.RateLimiting.RateLimiter>();
        secondLimiterMock.Protected().Setup<RateLimitLease>("AttemptAcquireCore", false, ItExpr.IsAny<int>())
            .Returns(mockLease.Object).Verifiable();
        var limiter = PartitionedRateLimiter.CreateChained(
            PredicatedRateLimiter.Create<int, int>(_ => false,
                _ => RateLimitPartition.Get(1, _ => firstLimiterMock.Object)),
            PredicatedRateLimiter.Create<int, int>(_ => true,
                _ => RateLimitPartition.Get(1, _ => secondLimiterMock.Object))
        );

        var lease = limiter.AttemptAcquire(1);

        firstLimiterMock.Protected().Verify("AttemptAcquireCore", Times.Never(), ItExpr.IsAny<int>());
        secondLimiterMock.Protected().Verify("AttemptAcquireCore", Times.Once(), ItExpr.IsAny<int>());
    }

    [Test]
    public void PredicateLeaseMetadataIsNull()
    {
        var limiter = PredicatedRateLimiter.Create<int, int>(_ => false, _ => RateLimitPartition.GetNoLimiter(1));

        var lease = limiter.AttemptAcquire(1);

        var success = lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter);
        Assert.That(success, Is.False);
        Assert.That(retryAfter, Is.Default);
    }

    [Test]
    public void PredicateLeaseMetadataNamesIsEmpty()
    {
        var limiter = PredicatedRateLimiter.Create<int, int>(_ => false, _ => RateLimitPartition.GetNoLimiter(1));

        var lease = limiter.AttemptAcquire(1);
        var mataDataNames = lease.MetadataNames;

        Assert.That(mataDataNames, Is.Empty);
    }

    [Test]
    public void PredicateLimiterGetStatisticsReturnsUnderlyingStats()
    {
        var limiter = PartitionedRateLimiter.CreateChained(
            PredicatedRateLimiter.CreatePredicatedTimeSinceLimiter<int, int>(_ => true, 1,
                _ => new TimeSinceLimiterOptions { AllowedPeriod = TimeSpan.FromMinutes(10) }),
            PredicatedRateLimiter.CreatePredicatedTimeSinceLimiter<int, int>(_ => true, 1,
                _ => new TimeSinceLimiterOptions { AllowedPeriod = TimeSpan.FromMicroseconds(1) })
        );

        limiter.AttemptAcquire(1);
        limiter.AttemptAcquire(1);
        var stats = limiter.GetStatistics(1);

        Assert.That(stats, Is.Not.Null);
        Assert.That(stats?.CurrentAvailablePermits, Is.EqualTo(0));
        Assert.That(stats?.TotalSuccessfulLeases, Is.EqualTo(1));
        Assert.That(stats?.TotalFailedLeases, Is.EqualTo(1));
    }

    [Test]
    public async Task PredicateLimiterCanAquireAsync()
    {
        var limiter = PredicatedRateLimiter.Create<int, int>(_ => true, _ => RateLimitPartition.GetNoLimiter(1));

        var lease = await limiter.AcquireAsync(1);

        Assert.That(lease.IsAcquired, Is.True);
    }

    [Test]
    public async Task PredicateLimiterBypassesWhenAsync()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedTimeSinceLimiter<int, int>(_ => false, 1,
            _ => new TimeSinceLimiterOptions { AllowedPeriod = TimeSpan.FromMinutes(10) });

        var lease = await limiter.AcquireAsync(1);
        Assert.That(lease.IsAcquired, Is.True);
        lease = await limiter.AcquireAsync(1);
        Assert.That(lease.IsAcquired, Is.True);
    }

    [Test]
    public void CanCreatePredicatedTokenBucketLimiter()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedTokenBucketLimiter<int, int>(_ => true, 1, _ =>
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 1,
                TokensPerPeriod = 1
            });
        Assert.That(limiter, Is.Not.Null);
    }

    [Test]
    public void CanCreatePredicatedConcurrencyLimiter()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedConcurrencyLimiter<int, int>(_ => true, 1, _ =>
            new ConcurrencyLimiterOptions
            {
                PermitLimit = 1
            });
        Assert.That(limiter, Is.Not.Null);
    }

    [Test]
    public void CanCreatePredicatedSlidingWindowLimiter()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedSlidingWindowLimiter<int, int>(_ => true, 1, _ =>
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 1
            });
        Assert.That(limiter, Is.Not.Null);
    }

    [Test]
    public void CanCreatePredicatedFixedWindowLimiter()
    {
        var limiter = PredicatedRateLimiter.CreatePredicatedFixedWindowLimiter<int, int>(_ => true, 1, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromMinutes(1)
            });
        Assert.That(limiter, Is.Not.Null);
    }
}