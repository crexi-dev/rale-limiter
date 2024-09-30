using RateLimiter;
using System;
using Xunit;

public class SlidingWindowLogRuleTests
{
    [Fact]
    public void SlidingWindowLogRule_AllowsRequestWithinLimit()
    {
        var rule = new SlidingWindowLogRule(5, TimeSpan.FromMinutes(1));
        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        for (int i = 0; i < 5; i++)
        {
            Assert.True(rule.IsRequestAllowed(context));
        }
    }

    [Fact]
    public void SlidingWindowLogRule_DeniesRequestExceedingLimit()
    {
        var rule = new SlidingWindowLogRule(5, TimeSpan.FromMinutes(1));
        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        for (int i = 0; i < 5; i++)
        {
            rule.IsRequestAllowed(context);
        }

        Assert.False(rule.IsRequestAllowed(context));
    }

    [Fact]
    public void SlidingWindowLogRule_AllowsRequestAfterOldRequestsExpire()
    {
        var rule = new SlidingWindowLogRule(5, TimeSpan.FromMinutes(1));
        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        for (int i = 0; i < 5; i++)
        {
            rule.IsRequestAllowed(context);
        }

        // Simulate waiting for some requests to expire
        context.RequestTime = context.RequestTime.AddSeconds(61);

        Assert.True(rule.IsRequestAllowed(context));
    }
}
