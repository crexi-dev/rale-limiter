using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using RateLimiter;

public class RateLimiterTests
{
    [Fact]
    public void RateLimitRuleA_AllowsRequestWithinLimit()
    {
        var rule = new RateLimitRuleA(5, TimeSpan.FromMinutes(1));
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
    public void RateLimitRuleA_DeniesRequestExceedingLimit()
    {
        var rule = new RateLimitRuleA(5, TimeSpan.FromMinutes(1));
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
    public void RateLimitRuleB_AllowsRequestAfterMinTimeSpan()
    {
        var rule = new RateLimitRuleB(TimeSpan.FromSeconds(30));
        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceB",
            RequestTime = DateTime.UtcNow
        };

        Assert.True(rule.IsRequestAllowed(context));

        context.RequestTime = context.RequestTime.AddSeconds(31);

        Assert.True(rule.IsRequestAllowed(context));
    }

    [Fact]
    public void RateLimitRuleB_DeniesRequestBeforeMinTimeSpan()
    {
        var rule = new RateLimitRuleB(TimeSpan.FromSeconds(30));
        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceB",
            RequestTime = DateTime.UtcNow
        };

        Assert.True(rule.IsRequestAllowed(context));

        context.RequestTime = context.RequestTime.AddSeconds(10);

        Assert.False(rule.IsRequestAllowed(context));
    }

    [Fact]
    public void RateLimitManager_AllowsRequestWithSingleRule()
    {
        var manager = new RateLimitManager();
        var ruleMock = new Mock<IRateLimitRule>();

        ruleMock.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitContext>())).Returns(true);

        manager.AddRule("resourceA", ruleMock.Object);

        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        Assert.True(manager.IsRequestAllowed(context));
    }

    [Fact]
    public void RateLimitManager_DeniesRequestWithSingleRule()
    {
        var manager = new RateLimitManager();
        var ruleMock = new Mock<IRateLimitRule>();

        ruleMock.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitContext>())).Returns(false);

        manager.AddRule("resourceA", ruleMock.Object);

        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        Assert.False(manager.IsRequestAllowed(context));
    }

    [Fact]
    public void RateLimitManager_AllowsRequestWithMultipleRules_AllSatisfied()
    {
        var manager = new RateLimitManager();
        var ruleMock1 = new Mock<IRateLimitRule>();
        var ruleMock2 = new Mock<IRateLimitRule>();

        ruleMock1.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitContext>())).Returns(true);
        ruleMock2.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitContext>())).Returns(true);

        manager.AddRule("resourceA", ruleMock1.Object);
        manager.AddRule("resourceA", ruleMock2.Object);

        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        Assert.True(manager.IsRequestAllowed(context));
    }

    [Fact]
    public void RateLimitManager_DeniesRequestWithMultipleRules_OneDenied()
    {
        var manager = new RateLimitManager();
        var ruleMock1 = new Mock<IRateLimitRule>();
        var ruleMock2 = new Mock<IRateLimitRule>();

        ruleMock1.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitContext>())).Returns(true);
        ruleMock2.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitContext>())).Returns(false);

        manager.AddRule("resourceA", ruleMock1.Object);
        manager.AddRule("resourceA", ruleMock2.Object);

        var context = new RateLimitContext
        {
            ClientToken = "client1",
            Resource = "resourceA",
            RequestTime = DateTime.UtcNow
        };

        Assert.False(manager.IsRequestAllowed(context));
    }
}
