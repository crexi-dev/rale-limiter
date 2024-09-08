using NUnit.Framework;
using NUnit.Framework.Internal;
using RateLimiter.Enums;
using RateLimiter.Service;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [Test]
    public void RuleA()
    {
        var tokenUS = "US12345";
        var tokenEU = "EU12345";

        var ruleA = new RuleA();
        ruleA.Configure("GET", 2, 5);

        ConfigureResources.Configure("GET", new List<RulesEnum> { RulesEnum.RuleA });
        var rules = ConfigureResources.GetResourceRules("GET");

        var rateLimiter = new RateLimiter();
        var testUS = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);
        var testUS2 = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);
        var testUS3 = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);
        var testUS4 = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now, rules);
       
        var testEU = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);
        var testEU2 = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);
        var testEU3 = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);
        var testEU4 = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now, rules);

        // First call 2 tokens.
        Assert.IsTrue(testUS);
        Assert.IsTrue(testEU);

        // Second call 2 tokens.
        Assert.IsTrue(testUS2);
        Assert.IsTrue(testEU2);

        // Third call 2 tokens false because configured for 2 calls.
        Assert.IsFalse(testUS3);
        Assert.IsFalse(testEU3);

        // Forth call 2 tokens true because more then 5 secs.  Counts as first call.
        Assert.IsTrue(testUS4);
        Assert.IsTrue(testEU4);
    }

    [Test]
    public void RuleB()
    {
        var tokenUS = "US12345";
        var tokenEU = "EU12345";

        var ruleB = new RuleB();
        ruleB.Configure("GET", 5);

        ConfigureResources.Configure("GET", new List<RulesEnum> { RulesEnum.RuleB });
        var rules = ConfigureResources.GetResourceRules("GET");

        var rateLimiter = new RateLimiter();
        var testUS = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);
        var testUS2 = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);
        var testUS3 = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now, rules);
       
        var testEU = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);
        var testEU2 = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);
        var testEU3 = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now, rules);

        // First call 2 tokens.
        Assert.IsTrue(testUS);
        Assert.IsTrue(testEU);

        // Second call 2 tokens false because configured for 5 secs between calls.
        Assert.IsFalse(testUS2);
        Assert.IsFalse(testEU2);

        // Third call 2 tokens true because more then 5 secs.  Counts as first call.
        Assert.IsTrue(testUS3);
        Assert.IsTrue(testEU3);
    }

    [Test]
    public void RuleC()
    {
        var tokenUS = "US12345";
        var tokenEU = "EU12345";

        var ruleA = new RuleA();
        ruleA.Configure("GET", 2, 5);

        var ruleB = new RuleB();
        ruleB.Configure("GET", 5);

        ConfigureResources.Configure("GET", new List<RulesEnum> { RulesEnum.RuleC });
        var rules = ConfigureResources.GetResourceRules("GET");

        var rateLimiter = new RateLimiter();
        var testUS = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);
        var testUS2 = rateLimiter.IsAllowed("GET", tokenUS, DateTime.Now.AddSeconds(-6), rules);

        var testEU = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);
        var testEU2 = rateLimiter.IsAllowed("GET", tokenEU, DateTime.Now.AddSeconds(-6), rules);


        // First call 2 tokens.
        Assert.IsTrue(testUS);
        Assert.IsTrue(testEU);

        // Second call US return RuleA. EU returns RuleB.
        Assert.IsTrue(testUS2);
        Assert.IsFalse(testEU2);
    }
}