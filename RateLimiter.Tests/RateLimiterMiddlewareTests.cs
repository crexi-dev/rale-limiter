using System;
using System.Threading.Tasks;
using NUnit.Framework;

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
        _ruleStorage.AddOrUpdateRuleAsync(RateLimiterRules.DatabaseRule).GetAwaiter().GetResult();
        _ruleStorage.AddOrUpdateRuleAsync(RateLimiterRules.ExpensiveApiRule).GetAwaiter().GetResult();

        _stateStorage = (IRateLimitStateStorage<int>)_ruleStorage;
        _algorithm = new FixedWindowAlgorithm(_stateStorage);
        _middleware = new RateLimiterMiddleware(_ruleStorage, _algorithm, _configuration);
    }

    [Test]
    public async Task Test_Bursts()
    {
        // Arrange
        RateLimitResult result;

        var maxRequests = 5;
        var windowDuration = TimeSpan.FromSeconds(1);

        var request = new RateLimiterRequest("database", new RateLimitDescriptor("type", "cosmos"));

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
        var windowDuration = TimeSpan.FromSeconds(5);
        var request = new RateLimiterRequest("research", new EmptyRateLimitDescriptor());

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
        await _ruleStorage.AddOrUpdateRuleAsync(RateLimiterRules.BlacklistRule);

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
        var request = new RateLimiterRequest("database", new EmptyRateLimitDescriptor());

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

        var request = new RateLimiterRequest("database", new EmptyRateLimitDescriptor());

        // Act
        var result = await middleware.HandleRequestAsync(request);

        // Assert
        Assert.IsTrue(result.IsRateLimited);
    }
}
