using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RejectRateLimiterTest
{
	[Test]
	public void Example()
	{
		Assert.That(true, Is.True);
	}

	[Test]
	public void AllowedLimitTest()
	{
		var callLimiter = new RejectRateLimiter(2,TimeSpan.FromSeconds(5));
		var limiterToken = "1:1";

		Assert.That(callLimiter.CheckRequest(limiterToken), Is.True);
		Assert.That(callLimiter.CheckRequest(limiterToken), Is.True);
		Assert.That(callLimiter.CheckRequest(limiterToken), Is.False);
		Thread.Sleep(TimeSpan.FromSeconds(5));
		Assert.That(callLimiter.CheckRequest(limiterToken), Is.True);
	}

	[Test]
	public void IntervalTest() { 
		var callLimiter = new RejectRateLimiter(1, TimeSpan.FromSeconds(5));
		var limiterToken = "1:1";
        Assert.That(callLimiter.CheckRequest(limiterToken), Is.True);
        Assert.That(callLimiter.CheckRequest(limiterToken), Is.False);
        Assert.That(callLimiter.CheckRequest(limiterToken), Is.False);
        Thread.Sleep(TimeSpan.FromSeconds(5));
        Assert.That(callLimiter.CheckRequest(limiterToken), Is.True);
    }

    [Test]
    public void Multithreaded_Test_For_RejectRateLimiter()
    {
        var callLimiter = new RejectRateLimiter(2, TimeSpan.FromSeconds(5));
        var limiterToken = "1:1";

        int successfulRequests = 0;
        int failedRequests = 0;

        // Define a multithreaded task
        var tasks = new Task[10];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                if (callLimiter.CheckRequest(limiterToken))
                {
                    Interlocked.Increment(ref successfulRequests);
                }
                else
                {
                    Interlocked.Increment(ref failedRequests);
                }
            });
        }

        // Wait for all tasks to complete
        Task.WaitAll(tasks);

        // Validate the results
        Assert.That(successfulRequests, Is.EqualTo(2)); // Only 2 requests should succeed
        Assert.That(failedRequests, Is.EqualTo(8));    // The rest should fail

        // Sleep for the time limit to reset
        Thread.Sleep(TimeSpan.FromSeconds(5));

        // Validate that after the time reset, a request succeeds
        Assert.That(callLimiter.CheckRequest(limiterToken), Is.True);
    }
}