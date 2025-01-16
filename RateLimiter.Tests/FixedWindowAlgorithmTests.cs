using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class FixedWindowAlgorithmTests
{
    private IRateLimiterRuleStorage _ruleStorage = null!;
    private IRateLimitStateStorage<int> _stateStorage = null!;
    private IRateLimitAlgorithm _algorithm = null!;

    [SetUp]
    public void SetUp()
    {
        _ruleStorage = new InMemoryRateLimiterStorage();
        _stateStorage = (IRateLimitStateStorage<int>)_ruleStorage;
        _ruleStorage.AddOrUpdateRuleAsync(RateLimiterRules.DatabaseRule).GetAwaiter().GetResult();
        _ruleStorage.AddOrUpdateRuleAsync(RateLimiterRules.ExpensiveApiRule).GetAwaiter().GetResult();

        _algorithm = new FixedWindowAlgorithm(_stateStorage);
    }

    [Test]
    public async Task Test_Bursts()
    {
        // Arrange
        var rule = await _ruleStorage.GetRuleAsync("database", new RateLimitDescriptor("type", "cosmos"));

        RateLimitResult result;

        // Act & Assert - should allow the requests
        for (var i = rule!.RateLimit.MaxRequests; i > 0; i--)
        {
            result = await _algorithm.HandleRequestAsync(rule);

            Assert.IsFalse(result.IsRateLimited);
            Assert.AreEqual(i - 1, result.RemainingRequests);
            Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);
        }

        // Act & Assert - should rate limit the request since it is over the request count
        result = await _algorithm.HandleRequestAsync(rule);

        Assert.IsTrue(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(rule.RateLimit.WindowDuration, result.RetryAfter);

        // Act & Assert - wait for at least the window duration and retry. It should succeed.
        await Task.Delay(rule.RateLimit.WindowDuration);

        result = await _algorithm.HandleRequestAsync(rule);

        Assert.IsFalse(result.IsRateLimited);
        Assert.AreEqual(4, result.RemainingRequests);
        Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);
    }

    [Test]
    public async Task Test_Single_Execution_Per_Window()
    {
        // Arrange
        var rule = await _ruleStorage.GetRuleAsync("research", new RateLimitDescriptor("dummy", "dummy"));

        var result = await _algorithm.HandleRequestAsync(rule!);

        Assert.IsFalse(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);

        // Act & Assert - should rate limit the request as there no more requests available in the window
        result = await _algorithm.HandleRequestAsync(rule!);

        Assert.IsTrue(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(rule!.RateLimit.WindowDuration, result.RetryAfter);

        // Act & Assert - wait for at least the window duration and retry. It should succeed.
        await Task.Delay(rule!.RateLimit.WindowDuration);

        result = await _algorithm.HandleRequestAsync(rule);

        Assert.IsFalse(result.IsRateLimited);
        Assert.AreEqual(0, result.RemainingRequests);
        Assert.AreEqual(TimeSpan.Zero, result.RetryAfter);
    }
}
