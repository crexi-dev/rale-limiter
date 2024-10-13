using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using RateLimiterNS.RateLimitRules;

[TestFixture]
public class RateLimiterTest
{
    private RateLimiterNS.RateLimiter.RateLimiter? _rateLimiter;

    [SetUp]
    public void SetUp()
    {
        string xmlFilePath = Path.Combine(AppContext.BaseDirectory, "RateLimiter.xml");
        _rateLimiter = RateLimiterNS.RateLimiter.RateLimiter.LoadFromConfiguration(xmlFilePath);
    }

    [Test]
    public void ValidTokenWithMultipleRules_ShouldAllowRequests_UnderLimit()
    {
        //max 5 requests per 1 minute and min 2 seconds between requests allowed
        string token = "token1";

        for (int i = 0; i < 5; i++)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        }
    }

    [Test]
    public void ValidTokenWithMultipleRules_ShouldDenyRequests_OverLimit()
    {
        //max 5 requests per 1 minute and min 2 seconds between requests allowed
        string token = "token1";

        for (int i = 0; i < 5; i++)
        {
            if (i == 0)
            {
                Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
            }
            Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));
        }
        // The sixth request should be denied
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));
    }

    [Test]
    public void ConcurrentRequests_ShouldBeHandledCorrectly()
    {
        // max 3 requests per 1 minute allowed
        string token = "token3";
        int allowedRequests = 3;
        var tasks = new List<Task<bool>>();

        // Fire multiple requests concurrently
        for (int i = 0; i < 6; i++)
        {
            tasks.Add(Task.Run(() => _rateLimiter!.IsRequestAllowed(token)));
        }

        Task.WhenAll(tasks).Wait();

        int allowedCount = tasks.Count(task => task.Result);
        Assert.IsTrue(allowedCount <= allowedRequests, $"Allowed requests exceeded: {allowedCount} > {allowedRequests}");
    }

    [Test]
    public void ValidTokenWithMultipleRules_ShouldAllowRequests_AfterWait()
    {
        //max 5 requests per 1 minute and min 2 seconds between requests allowed
        string token = "token1";

        // First request -> should be allowed
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        // Second request (immediately) -> should be denied
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));
        // Sleep for 2 seconds
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
        // Now it should be allowed
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
    }
    [Test]
    public void ValidTokenWithTimeSpanSinceLastRequestRule_ShouldEnforceRule()
    {
        // min 2 seconds between requests allowed
        string token = "token2";
        // First request -> should be allowed
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        // Second request (immediately) -> should be denied
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));
        // Sleep for 2 seconds
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
        // Now next requests should be allowed and denied
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));

    }

    [Test]
    public void ValidTokenWithRequestsPerTimeSpanRule_ShouldEnforceRule()
    {
        // max 3 requests per 1 minute allowed
        string token = "token3";

        // First request -> should be allowed
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));

        // Second request -> should be allowed
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));

        // Third request -> should be allowed
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));

        // Fourth request -> should be denied  (exceeding the limit of 3 requests within 1 minute)
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));

        // Wait for the timespan of 1 minute
        System.Threading.Thread.Sleep(TimeSpan.FromMinutes(1));

        // restriction should be lifted and next 3 requests should be allowed now and 4th request should be denied
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        Assert.IsTrue(_rateLimiter!.IsRequestAllowed(token));
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));
    }

    [Test]
    public void InvalidToken_ShouldDenyRequests()
    {
        string token = "invalid_token";
        Assert.IsFalse(_rateLimiter!.IsRequestAllowed(token));
    }
}
