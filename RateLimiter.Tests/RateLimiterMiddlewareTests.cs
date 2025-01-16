using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RateLimiter.Algorithms;
using RateLimiter.Domain;
using RateLimiter.Middleware;
using RateLimiter.Storage;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterMiddlewareTests
{
    private IRateLimiterRuleStorage _ruleStorage = null!;
    private IRateLimitStateStorage<int> _stateStorage = null!;
    private IRateLimitAlgorithm _algorithm = null!;
    private RateLimiterMiddleware _middleware = null!;
    private RateLimiterMiddlewareConfiguration _configuration = null!;

    [SetUp]
    public void SetUp()
    {
        _configuration = new RateLimiterMiddlewareConfiguration
        {
            NoAssociatedRuleBehaviorHandling = NoAssociatedRuleBehavior.AllowRequest
        };

        _ruleStorage = new InMemoryRateLimiterStorage();
        _stateStorage = (IRateLimitStateStorage<int>)_ruleStorage;
        _algorithm = new FixedWindowAlgorithm(_stateStorage);
        _middleware = new RateLimiterMiddleware(_ruleStorage, _algorithm, _configuration);
    }

    [Test]
    public async Task Test_Bursts()
    {
        // Arrange

        var rule = new RateLimitRule(
            "team-b",
            new[] { new RateLimitDescriptor("location", "us") },
            new RateLimit(5, TimeSpan.FromSeconds(1)));

        await _ruleStorage.AddOrUpdateRuleAsync(rule);

        RateLimitResult result;

        var maxRequests = rule.RateLimit.MaxRequests;
        var windowDuration = rule.RateLimit.WindowDuration;

        var request = new RateLimiterRequest("team-b", new RateLimitDescriptor("location", "us"));

        // Act & Assert - should allow the requests
        for (var i = maxRequests; i > 0; i--)
        {
            result = await _middleware.HandleRequestAsync(request);

            Assert.IsFalse(result.IsRateLimited);
            Assert.AreEqual(i - 1, result.RemainingRequests);
            Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);
        }

        // Act & Assert - should rate limit the request since it is over the request count
        result = await _middleware.HandleRequestAsync(request);

        Assert.IsTrue(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(windowDuration, result.RetryAfter);

        // Act & Assert - wait for at least the window duration and retry. It should succeed.
        await Task.Delay(windowDuration);

        result = await _middleware.HandleRequestAsync(request);

        Assert.IsFalse(result.IsRateLimited);
        Assert.AreEqual(4, result.RemainingRequests);
        Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);
    }

    [Test]
    public async Task Test_Single_Execution_Per_Window()
    {
        // Arrange
        var rule = new RateLimitRule(
            "team-b",
            new[] { new RateLimitDescriptor("location", "eu") },
            new RateLimit(1, TimeSpan.FromSeconds(5)));

        await _ruleStorage.AddOrUpdateRuleAsync(rule);

        var windowDuration = rule.RateLimit.WindowDuration;
        var request = new RateLimiterRequest("team-b", new RateLimitDescriptor("location", "eu"));

        var result = await _middleware.HandleRequestAsync(request);

        Assert.IsFalse(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);

        // Act & Assert - should rate limit the request as there no more requests available in the window
        result = await _middleware.HandleRequestAsync(request);

        Assert.IsTrue(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(windowDuration, result.RetryAfter);

        // Act & Assert - wait for at least the window duration and retry. It should succeed.
        await Task.Delay(windowDuration);

        result = await _middleware.HandleRequestAsync(request);

        Assert.IsFalse(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);
    }

    [Test]
    public async Task Blacklist_Rules_Rate_Limits_All_Requests()
    {
        // Arrange
        var rule = new RateLimitRule(
            "payment",
            new[]
            {
                new RateLimitDescriptor("userid", "foobar1@gmail.com"),
                new RateLimitDescriptor("userid", "foobar2@gmail.com")
            },
            new RateLimit(0, TimeSpan.Zero));

        await _ruleStorage.AddOrUpdateRuleAsync(rule);

        var request1 = new RateLimiterRequest("payment", new RateLimitDescriptor("userid", "foobar1@gmail.com"));
        var request2 = new RateLimiterRequest("payment", new RateLimitDescriptor("userid", "foobar2@gmail.com"));

        // Act
        var allTasks = Task.WhenAll(
            _middleware.HandleRequestAsync(request1),
            _middleware.HandleRequestAsync(request2));

        var results = await allTasks;

        // Assert
        foreach (var result in results)
        {
            Assert.IsTrue(result.IsRateLimited);
        }
    }

    [Test]
    public async Task Should_Allow_Requests_Not_Associated_With_A_Rule()
    {
        // Arrange
        var request = new RateLimiterRequest("foo", new EmptyRateLimitDescriptor());

        // Act
        var result = await _middleware.HandleRequestAsync(request);

        // Assert
        Assert.IsFalse(result.IsRateLimited);
    }

    [Test]
    public async Task Should_RateLimit_Requests_Not_Associated_With_A_Rule_If_So_Configured()
    {
        // Arrange
        var configuration = new RateLimiterMiddlewareConfiguration
        {
            NoAssociatedRuleBehaviorHandling = NoAssociatedRuleBehavior.RateLimitRequest
        };

        var middleware = new RateLimiterMiddleware(
            _ruleStorage,
            _algorithm,
            configuration);

        var request = new RateLimiterRequest("foo", new EmptyRateLimitDescriptor());

        // Act
        var result = await middleware.HandleRequestAsync(request);

        // Assert
        Assert.IsTrue(result.IsRateLimited);
    }

    [Test]
    public async Task Test_Rule_With_Multiple_Descriptors()
    {
        // Arrange
        var rule = new RateLimitRule(
            "marketing",
            new[] { new RateLimitDescriptor("phone", "555-1234"), new RateLimitDescriptor("phone", "555-5678") },
            new RateLimit(2, TimeSpan.FromSeconds(1)));

        await _ruleStorage.AddOrUpdateRuleAsync(rule);

        var request1 = new RateLimiterRequest("marketing", new RateLimitDescriptor("phone", "555-1234"));
        var request2 = new RateLimiterRequest("marketing", new RateLimitDescriptor("phone", "555-5678"));
        var request3 = new RateLimiterRequest("marketing", new RateLimitDescriptor("phone", "555-1234"));

        // Act
        var result1 = await _middleware.HandleRequestAsync(request1);
        var result2 = await _middleware.HandleRequestAsync(request2);
        var result3 = await _middleware.HandleRequestAsync(request3);

        // Assert
        Assert.IsFalse(result1.IsRateLimited);
        Assert.IsFalse(result2.IsRateLimited);
        Assert.IsTrue(result3.IsRateLimited);

        // Wait for a second and retry - should not be rate limited
        await Task.Delay(rule.RateLimit.WindowDuration);
        result1 = await _middleware.HandleRequestAsync(request1);

        Assert.IsFalse(result1.IsRateLimited);

    }
}
