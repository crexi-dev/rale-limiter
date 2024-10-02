using RateLimiter;
using System;
using Xunit;

public class FixedWindowRuleTests
{
    [Fact]
    public void FixedWindowRule_AllowsRequestWithinLimit()
    {
        var rule = new FixedWindowRule(5, TimeSpan.FromMinutes(1));
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
    public void FixedWindowRule_DeniesRequestExceedingLimit()
    {
        var rule = new FixedWindowRule(5, TimeSpan.FromMinutes(1));
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
    public void FixedWindowRule_AllowsRequestAfterWindowReset()
    {
        var rule = new FixedWindowRule(5, TimeSpan.FromMinutes(1));
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

        // Simulate waiting for the window to reset
        context.RequestTime = context.RequestTime.AddMinutes(1);

        Assert.True(rule.IsRequestAllowed(context));
    }
}
