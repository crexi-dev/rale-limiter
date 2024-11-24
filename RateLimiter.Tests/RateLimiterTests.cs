using Crexi.API.Common.RateLimiter;
using Crexi.API.Common.RateLimiter.Models;
using Crexi.API.Common.RateLimiter.Rules;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

[TestFixture]
public class RateLimiterTests
{
    [Test]
    public void FixedWindowRateLimitRule_Allows_Under_Limit()
    {
        var maxRequests = 5;
        var timeWindow = TimeSpan.FromMinutes(1);
        var rule = new FixedWindowRateLimitRule(maxRequests, timeWindow);
        var client = new Client("client1", "US");
        bool allowed = true;

        for (int i = 0; i < maxRequests; i++)
        {
            allowed &= rule.IsRequestAllowed(client, "ResourceA");
        }

        Assert.IsTrue(allowed);
    }

    [Test]
    public void FixedWindowRateLimitRule_Blocks_Over_Limit()
    {
        var maxRequests = 5;
        var timeWindow = TimeSpan.FromMinutes(1);

        var rule = new FixedWindowRateLimitRule(maxRequests, timeWindow);
        var client = new Client("client1", "US");

        for (int i = 0; i < maxRequests; i++)
        {
            rule.IsRequestAllowed(client, "ResourceA");
        }

        bool allowed = rule.IsRequestAllowed(client, "ResourceA");
        Assert.IsFalse(allowed);
    }

    [Test]
    public void TimeSinceLastCallRule_Allows_After_Interval()
    {
        var rule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(1));
        var client = new Client("client1", "EU");

        bool firstCall = rule.IsRequestAllowed(client, "ResourceA");
        Task.Delay(1100).Wait(); // Wait for more than 1 second
        bool secondCall = rule.IsRequestAllowed(client, "ResourceA");

        Assert.IsTrue(firstCall);
        Assert.IsTrue(secondCall);
    }

    [Test]
    public void TimeSinceLastCallRule_Blocks_Before_Interval()
    {
        var interval = TimeSpan.FromSeconds(1);
        var rule = new TimeSinceLastCallRule(interval);
        var client = new Client("client1", "EU");

        bool firstCall = rule.IsRequestAllowed(client, "ResourceA");
        bool secondCall = rule.IsRequestAllowed(client, "ResourceA");

        Assert.IsTrue(firstCall);
        Assert.IsFalse(secondCall);
    }

    [Test]
    public void ConditionalRateLimitRule_Applies_Correct_Rules()
    {
        var maxRequests = 2;
        var usaTimeWindow = TimeSpan.FromMinutes(1);
        var usRule = new FixedWindowRateLimitRule(maxRequests, usaTimeWindow);
        var euInterval = TimeSpan.FromSeconds(1);
        var euRule = new TimeSinceLastCallRule(euInterval);

        var usConditionalRule = new ConditionalRateLimitRule(
            client => client.Location == "US",
            usRule
        );

        var euConditionalRule = new ConditionalRateLimitRule(
            client => client.Location == "EU",
            euRule
        );

        var compositeRule = new CompositeRateLimitRule(new[] { usConditionalRule, euConditionalRule });
        var clientUS = new Client("clientUS", "US");
        var clientEU = new Client("clientEU", "EU");

        // US client tests
        Assert.IsTrue(compositeRule.IsRequestAllowed(clientUS, "ResourceA"));
        Assert.IsTrue(compositeRule.IsRequestAllowed(clientUS, "ResourceA"));
        Assert.IsFalse(compositeRule.IsRequestAllowed(clientUS, "ResourceA"));

        // EU client tests
        Assert.IsTrue(compositeRule.IsRequestAllowed(clientEU, "ResourceA"));
        Assert.IsFalse(compositeRule.IsRequestAllowed(clientEU, "ResourceA"));
        Task.Delay(1100).Wait(); // Wait for more than 1 second
        Assert.IsTrue(compositeRule.IsRequestAllowed(clientEU, "ResourceA"));
    }

    [Test]
    public void RateLimiter_Allows_And_Blocks_As_Configured()
    {
        var rateLimiter = new RateLimiter();
        var maxRequests = 2;
        var timeWindow = TimeSpan.FromMinutes(1);
        var usRule = new FixedWindowRateLimitRule(maxRequests, timeWindow);
        var usConditionalRule = new ConditionalRateLimitRule(
            client => client.Location == "US",
            usRule
        );

        rateLimiter.ConfigureResource("ResourceA", usConditionalRule);

        var clientUS = new Client("clientUS", "US");

        Assert.IsTrue(rateLimiter.IsRequestAllowed(clientUS, "ResourceA"));
        Assert.IsTrue(rateLimiter.IsRequestAllowed(clientUS, "ResourceA"));
        Assert.IsFalse(rateLimiter.IsRequestAllowed(clientUS, "ResourceA"));
    }
}
