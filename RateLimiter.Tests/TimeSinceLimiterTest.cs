using System;
using System.Linq;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class TimeSinceLimiterTest
{
	[Test]
	public void CanAcquireLease()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMicroseconds(1)});
		
		var lease = limiter.AttemptAcquire();
		
		Assert.That( lease.IsAcquired, Is.True );
	}

	[Test]
	public void InvalidOptionsThrows()
	{
		Assert.Throws<ArgumentException>(() => new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.Zero}));
	}
	
	[Test]
	public void CanAcquireLeaseAfterPeriod()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromSeconds(1)});
		
		var lease = limiter.AttemptAcquire();
		
		Assert.That( lease.IsAcquired, Is.True );
		
		Thread.Sleep(TimeSpan.FromSeconds(1.1));
		lease = limiter.AttemptAcquire();
		Assert.That( lease.IsAcquired, Is.True );
	}
	
	[Test]
	public void CannotAcquireLeaseWithinPeriod()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromSeconds(1)});
		
		var lease = limiter.AttemptAcquire();
		Assert.That( lease.IsAcquired, Is.True );
		
		lease = limiter.AttemptAcquire();
		Assert.That( lease.IsAcquired, Is.False );
	}

	[Test]
	public void MetadataContainsRetryAfter()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMinutes(1)});
		
		var lease = limiter.AttemptAcquire();
		Assert.That(lease.IsAcquired, Is.True);
		lease = limiter.AttemptAcquire();
		
		Assert.That(lease.IsAcquired, Is.False);
		Assert.That( lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter), Is.True );
		Assert.That( retryAfter, Is.LessThan(TimeSpan.FromMinutes(1)) );
	}

	[Test]
	public void MetadataReturnsDefaultWhenNotRetryAfter()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMinutes(1)});
		
		var lease = limiter.AttemptAcquire();
		Assert.That(lease.IsAcquired, Is.True);
		lease = limiter.AttemptAcquire();
		
		Assert.That(lease.IsAcquired, Is.False);
		Assert.That( lease.TryGetMetadata(MetadataName.ReasonPhrase, out var reasonPhrase), Is.False );
		Assert.That( reasonPhrase, Is.Null);
	}

	[Test]
	public void MetadataNamesReturnsRetryAfter()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMinutes(1)});
		var lease = limiter.AttemptAcquire();
		
		var names = lease.MetadataNames;
		
		Assert.That(names.Count(), Is.EqualTo(1));
		Assert.That(names, Has.Member(MetadataName.RetryAfter.Name));
	}

	[Test]
	public void RetrievingMoreThanOneLeaseThrows()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMinutes(1)});
		Assert.Throws<ArgumentOutOfRangeException>(() => limiter.AttemptAcquire(2));
	}

	[Test]
	public void IdleDirationReturns()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMinutes(1)});
		
		Thread.Sleep(TimeSpan.FromMilliseconds(10));
		
		Assert.That(limiter.IdleDuration, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(10)));
	}

	[Test]
	public void GetStatisticsReturns()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMinutes(1)});
		limiter.AttemptAcquire();
		limiter.AttemptAcquire();
		
		var stats = limiter.GetStatistics();
		
		Assert.That(stats.CurrentAvailablePermits, Is.EqualTo(0));
		Assert.That(stats.TotalSuccessfulLeases, Is.EqualTo(1));
		Assert.That(stats.TotalFailedLeases, Is.EqualTo(1));
	}

	[Test]
	public async Task CanAcquireLeaseAsync()
	{
		var limiter = new TimeSinceLimiter(new TimeSinceLimiterOptions {AllowedPeriod = TimeSpan.FromMicroseconds(1)});
		
		var lease = await limiter.AcquireAsync();
		
		Assert.That( lease.IsAcquired, Is.True );
	}
}