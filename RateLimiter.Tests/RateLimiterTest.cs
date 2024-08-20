using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using RateLimiter.Contracts;
using RateLimiter.Services;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [TestCase(5, 10)]
    [TestCase(10, 20)]
    public void XRequestsPerTimespanRule_AllowsRequestsWithLimit(int maxRequests, int timespan)
    {
        var rule = new XRequestsPerTimespanRule(maxRequests, TimeSpan.FromSeconds(timespan));
        var clientToken = Guid.NewGuid().ToString("N");
        var allowedCount = 0;

        for (int i = 0; i < maxRequests; i++)
        {
            Assert.IsTrue(rule.IsRequestAllowed(clientToken));
        }
        
        Assert.IsFalse(rule.IsRequestAllowed(clientToken));
    }

    [TestCase(10)]
    [TestCase(15)]
    public void TimespanSinceLastCallRule_AllowsRequestAfterTimespan(int timespan)
    {
        var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(timespan));
        var clientToken = Guid.NewGuid().ToString("N");
        
        Assert.IsTrue(rule.IsRequestAllowed(clientToken));
        Assert.IsFalse(rule.IsRequestAllowed(clientToken));

        System.Threading.Thread.Sleep(5000);
        Assert.IsFalse(rule.IsRequestAllowed(clientToken));
    }
    
    [Test]
    public void RateLimiter_AllowsRequestsIfAllRullesPass()
    {
        var rules = new List<IRateLimitRule>
        {
            new TimespanSinceLastCallRule(TimeSpan.FromSeconds(2)),
            new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(20))
        };

        var rateLimiter = new Services.RateLimiter(rules);
        var clientToken = Guid.NewGuid().ToString("N");

        for (int i = 0; i < 5; i++)
        {
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientToken));
            Thread.Sleep(2000);
        }
        
        Assert.IsFalse(rateLimiter.IsRequestAllowed(clientToken));
        
        Thread.Sleep(10000);
        
        Assert.IsTrue(rateLimiter.IsRequestAllowed(clientToken));
    }
}