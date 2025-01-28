using System;
using NUnit.Framework;
using RateLimiter.Core.Configuration;
using RateLimiter.Core.Rules;
using RateLimiter.Core.Rules.Combine;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [Test]
    public void FixedWindowRule_AllowsMaxRequestsPerWindow()
    {
        var startTime = new DateTime(2025, 1, 28, 1, 1, 1, DateTimeKind.Utc);
        var logger = new ConsoleLogger();
        var clock = new MockSystemTime(startTime);
        var rule = new FixedWindowRule(2, TimeSpan.FromMinutes(1), clock, logger);
        var clientToken = "client1";
        var resourceKey = "resource1";

        Assert.IsTrue(rule.IsAllowed(clientToken, resourceKey));
        rule.RecordRequest(clientToken, resourceKey);

        Assert.IsTrue(rule.IsAllowed(clientToken, resourceKey));
        rule.RecordRequest(clientToken, resourceKey);

        Assert.IsFalse(rule.IsAllowed(clientToken, resourceKey));

        clock.CurrentTime = startTime.AddMinutes(1);
        Assert.IsFalse(rule.IsAllowed(clientToken, resourceKey));
    }

    [Test]
    public void TimeSinceLastCallRule_AllowsRequestAfterDelay()
    {
        var logger = new ConsoleLogger();
        var startTime = new DateTime(2025, 1, 28, 2, 2, 2, DateTimeKind.Utc);
        var clock = new MockSystemTime(startTime);
        var rule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(10), clock, logger);
        var clientToken = "client1";
        var resourceKey = "resource1";

        Assert.IsTrue(rule.IsAllowed(clientToken, resourceKey));
        rule.RecordRequest(clientToken, resourceKey);

        clock.CurrentTime = startTime.AddSeconds(5);
        Assert.IsFalse(rule.IsAllowed(clientToken, resourceKey));

        clock.CurrentTime = startTime.AddSeconds(10);
        Assert.IsTrue(rule.IsAllowed(clientToken, resourceKey));
    }

    [Test]
    public void RateLimiter_CombinedRules_BothMustAllow()
    {
        var startTime = new DateTime(2025, 1, 28, 3, 3, 3, DateTimeKind.Utc);
        var clock = new MockSystemTime(startTime);
        var logger = new ConsoleLogger();
        var rateLimiter = new Core.Configuration.RateLimiter(logger);
        rateLimiter.AddRule("resource1", new FixedWindowRule(2, TimeSpan.FromMinutes(1), clock, logger));
        rateLimiter.AddRule("resource1", new TimeSinceLastCallRule(TimeSpan.FromSeconds(10), clock, logger));

        var clientToken = "client1";
        Assert.IsTrue(rateLimiter.IsRequestAllowed(clientToken, "resource1"));
        clock.CurrentTime = startTime.AddSeconds(5);
        Assert.IsFalse(rateLimiter.IsRequestAllowed(clientToken, "resource1"));

        clock.CurrentTime = startTime.AddSeconds(10);
        Assert.IsTrue(rateLimiter.IsRequestAllowed(clientToken, "resource1"));
    }

    [Test]
    public void RegionBasedRule_UsesDifferentRulesBasedOnTokenRegion()
    {
        var logger = new ConsoleLogger();
        var startTime = new DateTime(2025, 1, 28, 4, 4, 4, DateTimeKind.Utc);
        var clock = new MockSystemTime(startTime);
        var usRule = new FixedWindowRule(2, TimeSpan.FromMinutes(1), clock, logger);
        var euRule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(10), clock, logger);
        var regionRule = new RegionBasedRule(usRule, euRule);

        var rateLimiter = new Core.Configuration.RateLimiter(logger);
        rateLimiter.AddRule("resource1", regionRule);

        var usToken = "US-123";
        var euToken = "EU-321";
     
        Assert.IsTrue(rateLimiter.IsRequestAllowed(usToken, "resource1"));
        Assert.IsTrue(rateLimiter.IsRequestAllowed(usToken, "resource1"));
        Assert.IsFalse(rateLimiter.IsRequestAllowed(usToken, "resource1"));

        Assert.IsTrue(rateLimiter.IsRequestAllowed(euToken, "resource1"));
        Assert.IsFalse(rateLimiter.IsRequestAllowed(euToken, "resource1"));
    }
}