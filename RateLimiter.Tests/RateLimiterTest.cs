using Moq;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Factory;
using RateLimiter.Model;
using RateLimiter.Rules;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private RateLimiterManager _rateLimiterManager;
    private IRateLimiterRuleFactory _rateLimiterRuleFactory;
    private IRateLimiterDataStore _rateLimitterDataStore;

    [SetUp]
    public void Setup()
    {
        // In-memory data store
        _rateLimitterDataStore = new RateLimiterDataStore();

        // Set up the factory with data store
        _rateLimiterRuleFactory = new RateLimiterRuleFactory(_rateLimitterDataStore);

        // Set up the RateLimitManager with the factory
        _rateLimiterManager = new RateLimiterManager(_rateLimiterRuleFactory, _rateLimitterDataStore);
    }

    [Test]
    public void Example()
    {
        Assert.That(true, Is.True);
    }

    [Test]
    public void RateLimiter_Should_Allow_Requests_Until_Limit_Exceeded_regular_client()
    {
        var clientId = "regular-client";
        var resource = "/api/resource";

        for (int i = 0; i < 205; i++)
        {
            var result = _rateLimiterManager.CheckRequest(clientId, resource);
            if (i <= 200)
            {
                Assert.True(result.IsAllowed);
            }
            else
            {
                Assert.False(result.IsAllowed);
            }
        }
    }

    [Test]
    public void RateLimiter_Should_Allow_Requests_Until_Limit_Exceeded_basic_client()
    {
        var clientId = "basic-client";
        var resource = "/api/resource";

        for (int i = 0; i < 105; i++)
        {
            var result = _rateLimiterManager.CheckRequest(clientId, resource);
            if (i <= 100)
            {
                Assert.True(result.IsAllowed);
            }
            else
            {
                Assert.False(result.IsAllowed);
            }
        }
    }

    [Test]
    public void RateLimiter_Should_Allow_Requests_Until_Limit_Exceeded_premium_client()
    {
        var clientId = "premium-client";
        var resource = "/api/resource";

        for (int i = 0; i < 505; i++)
        {
            var result = _rateLimiterManager.CheckRequest(clientId, resource);
            if (i <= 500)
            {
                Assert.True(result.IsAllowed);
            }
            else
            {
                Assert.False(result.IsAllowed);
            }
        }
    }

    [Test]
    public void RateLimiter_Should_Allow_Requests_Until_Limit_Exceeded_free_client()
    {
        var clientId = "free-client";
        var resource = "/api/resource";

        for (int i = 0; i < 10; i++)
        {
            var result = _rateLimiterManager.CheckRequest(clientId, resource);
            if (i == 0)// From second request should fail as the Rule has a limit of 5 second apart between request
            {
                Assert.True(result.IsAllowed);
            }
            else
            {
                Assert.False(result.IsAllowed);
            }
        }
    }

    [Test]
    public void RateLimiter_Should_Allow_Requests_Until_Limit_Exceeded_free_client_with_Delay()
    {
        var clientId = "free-client";
        var resource = "/api/resource";

        for (int i = 0; i < 5; i++)
        {
            var result = _rateLimiterManager.CheckRequest(clientId, resource);
            Assert.True(result.IsAllowed);
            Thread.Sleep(6000);
        }
    }

    [Test]
    public void Concurrency_Thread_SafetyIn_ParallelOperation()
    {
        var rateLimiter = new RateLimiterDataStore();

        Parallel.For(0, 1000, i =>
        {
            rateLimiter.IncrementRequestCount("base-clientId", "resource");
        });

        var clientData = rateLimiter.GetClientData("base-clientId", "resource");
        Assert.AreEqual(clientData.RequestCount, 1000); // Ensure no race condition
    }

    [Test]
    public void Thread_SafetyIn_Loop()
    {
        var rateLimiter = new RateLimiterDataStore();

        for (int i = 0; i < 1000; i++)
        {
            rateLimiter.IncrementRequestCount("base-clientId", "resource");
        }
        var clientData = rateLimiter.GetClientData("base-clientId", "resource");
        Assert.AreEqual(clientData.RequestCount, 1000);
    }
}