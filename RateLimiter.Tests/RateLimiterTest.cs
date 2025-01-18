using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

// I went with an easy implementation to not imitate API requests
// and also to not use a test repository for storing clients and resources.

[TestFixture]
public class RateLimiterTest
{
    [Test]
    public void AllowsRequestsWithinLimit()
    {
        var clientId = "client1";
        var resource = "resource1";

        var ruleSet = new RuleSet();
        ruleSet.AddRule(new FixedCountPerTimeSpanRule(5, TimeSpan.FromMinutes(1)));

        var rateLimiter = new RateLimiter();
        rateLimiter.Configure(resource, ruleSet);

        for (var i = 0; i < 5; i++)
        {
            Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
        }

        // The 6th request should be denied
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.False);
    }

    [Test]
    public void ResetsAfterTimeSpan()
    {
        var clientId = "client1";
        var resource = "resource1";

        var ruleSet = new RuleSet();
        ruleSet.AddRule(new FixedCountPerTimeSpanRule(2, TimeSpan.FromSeconds(1)));

        var rateLimiter = new RateLimiter();
        rateLimiter.Configure(resource, ruleSet);

        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);

        // Exceeds limit
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.False);

        // Wait for the time span to reset
        Thread.Sleep(1111);
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
    }

    [Test]
    public void AllowsRequestsWithSufficientInterval()
    {
        var clientId = "client1";
        var resource = "resource1";

        var ruleSet = new RuleSet();
        ruleSet.AddRule(new MinTimeBetweenRequestsRule(TimeSpan.FromSeconds(1)));

        var rateLimiter = new RateLimiter();
        rateLimiter.Configure(resource, ruleSet);

        // First request
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
        // Too soon
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.False);

        // Wait for the interval
        Thread.Sleep(1111);
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
    }

    [Test]
    public void CombinesMultipleRules()
    {
        var clientId = "client1";
        var resource = "resource1";

        var ruleSet = new RuleSet();
        ruleSet.AddRule(new FixedCountPerTimeSpanRule(3, TimeSpan.FromMinutes(1)));
        ruleSet.AddRule(new MinTimeBetweenRequestsRule(TimeSpan.FromSeconds(1)));

        var rateLimiter = new RateLimiter();
        rateLimiter.Configure(resource, ruleSet);

        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
        Thread.Sleep(1111);
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);
        Thread.Sleep(1111);
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.True);

        // Exceeds limit
        Assert.That(rateLimiter.IsAllowed(clientId, resource), Is.False);
    }

    [Test]
    public void AllowsDifferentRulesPerResource()
    {
        var clientId = "client1";
        var resource1 = "resource1";
        var resource2 = "resource2";

        var rateLimiter = new RateLimiter();

        var resource1Rules = new RuleSet();
        resource1Rules.AddRule(new FixedCountPerTimeSpanRule(2, TimeSpan.FromMinutes(1)));

        var resource2Rules = new RuleSet();
        resource2Rules.AddRule(new MinTimeBetweenRequestsRule(TimeSpan.FromSeconds(2)));

        rateLimiter.Configure(resource1, resource1Rules);
        rateLimiter.Configure(resource2, resource2Rules);

        // Resource 1
        Assert.That(rateLimiter.IsAllowed(clientId, resource1), Is.True);
        Assert.That(rateLimiter.IsAllowed(clientId, resource1), Is.True);

        // Exceeds limit
        Assert.That(rateLimiter.IsAllowed(clientId, resource1), Is.False);


        // Resource 2
        Assert.That(rateLimiter.IsAllowed(clientId, resource2), Is.True);

        // Too soon
        Assert.That(rateLimiter.IsAllowed(clientId, resource2), Is.False);

        Thread.Sleep(2222);
        Assert.That(rateLimiter.IsAllowed(clientId, resource2), Is.True);
    }

    [Test]
    public void AppliesPriorityLimitsUsingRateLimiterAndRuleSet()
    {
        var resource = "resource1";
        var clientId1 = "client1_high_priority";
        var clientId2 = "client2_normal_priority";

        var clientPriorities = new Dictionary<string, int>
        {
            { clientId1, 2 },
            { clientId2, 1 }
        };

        var ruleSet = new RuleSet();
        ruleSet.AddRule(new ClientPriorityRule(3, TimeSpan.FromMinutes(1), clientPriorities));

        var rateLimiter = new RateLimiter();
        rateLimiter.Configure(resource, ruleSet);

        // Normal priority client
        for (var i = 0; i < 3; i++)
        {
            Assert.That(rateLimiter.IsAllowed(clientId2, resource), Is.True);
        }

        // Exceeds limit
        Assert.That(rateLimiter.IsAllowed(clientId2, resource), Is.False);


        // High priority client
        for (var i = 0; i < 6; i++)
        {
            Assert.That(rateLimiter.IsAllowed(clientId1, resource), Is.True);
        }

        // Exceeds limit
        Assert.That(rateLimiter.IsAllowed(clientId1, resource), Is.False);
    }
}
