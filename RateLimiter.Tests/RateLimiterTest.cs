using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void WindowCounterSuccessTest1()
	{
        //arrange
		var rateLimiter = new RateLimiter();
		var resourceA = "/testA/";
        var rules = new List<Rule>();
        var rule = new Rule();
        rule.RuleAlg = new WindowCounterRule(3, 3);

		rules.Add(rule);

		rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequests(resourceA, 3, accessToken);

        //assert
		Assert.That(res1, Is.True);
    }

    [Test]
    public void WindowCounterFailureTest1()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var rule = new Rule();
        rule.RuleAlg = new WindowCounterRule(3, 3);

        rules.Add(rule);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequests(resourceA, 4, accessToken);

        //assert
        Assert.That(res1, Is.False);
    }

    [Test]
    public void WindowCounterSuccessTest2()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var rule = new Rule();
        rule.RuleAlg = new WindowCounterRule(3, 3);

        rules.Add(rule);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequests(resourceA, 3, accessToken);

        Assert.That(res1, Is.True);

        Thread.Sleep(4000);

        var res2 = rateLimiter.SendRequest(resourceA, accessToken);

        Assert.That(res2, Is.True);
    }

    [Test]
    public void TimeSpanSuccessTest1()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var rule = new Rule();
        rule.RuleAlg = new BasicTimeSpanRule( TimeSpan.FromSeconds(3) );

        rules.Add(rule);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequest(resourceA, accessToken);

        //assert
        Assert.That(res1, Is.True);
    }

    [Test]
    public void TimeSpanFailureTest1()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var rule = new Rule();
        rule.RuleAlg = new BasicTimeSpanRule(TimeSpan.FromSeconds(3));

        rules.Add(rule);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequest(resourceA, accessToken);

        Thread.Sleep(2000);

        var res2 = rateLimiter.SendRequest(resourceA, accessToken);

        //assert
        Assert.That(res2, Is.False);
    }

    [Test]
    public void TimeSpanSuccessTest2()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var rule = new Rule();
        rule.RuleAlg = new BasicTimeSpanRule(TimeSpan.FromSeconds(3));

        rules.Add(rule);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequest(resourceA, accessToken);

        Thread.Sleep(3500);

        var res2 = rateLimiter.SendRequest(resourceA, accessToken);

        //assert
        Assert.That(res2, Is.True);
    }

    [Test]
    public void WinterCounterAndBasicTimeSpanSuccessTest1()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var ruleA = new Rule();
        ruleA.RuleAlg = new WindowCounterRule(3, 3);
        var ruleB = new Rule();
        ruleB.RuleAlg = new BasicTimeSpanRule(TimeSpan.FromSeconds(1));

        rules.Add(ruleA);
        rules.Add(ruleB);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequest(resourceA, accessToken);

        Thread.Sleep(1000);

        var res2 = rateLimiter.SendRequest(resourceA, accessToken);

        Thread.Sleep(1000);

        var res3 = rateLimiter.SendRequest(resourceA, accessToken);

        //assert
        Assert.That(res1, Is.True);
        Assert.That(res2, Is.True);
        Assert.That(res3, Is.True);
    }

    [Test]
    public void WinterCounterAndBasicTimeSpanFailureTest1()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var ruleA = new Rule();
        ruleA.RuleAlg = new WindowCounterRule(3, 3);
        var ruleB = new Rule();
        ruleB.RuleAlg = new BasicTimeSpanRule(TimeSpan.FromSeconds(1));

        rules.Add(ruleA);
        rules.Add(ruleB);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequests(resourceA, 3, accessToken);

        //assert
        Assert.That(res1, Is.False);
    }

    [Test]
    public void WinterCounterAndBasicTimeSpanFailureTest2()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var ruleA = new Rule();
        ruleA.RuleAlg = new WindowCounterRule(4, 2);
        var ruleB = new Rule();
        ruleB.RuleAlg = new BasicTimeSpanRule(TimeSpan.FromSeconds(1));

        rules.Add(ruleA);
        rules.Add(ruleB);

        rateLimiter.SetRules(resourceA, rules);

        var accessToken = "US1234";

        //act
        var res1 = rateLimiter.SendRequest(resourceA, accessToken);

        Thread.Sleep(1000);

        var res2 = rateLimiter.SendRequest(resourceA, accessToken);

        Thread.Sleep(1000);

        var res3 = rateLimiter.SendRequest(resourceA, accessToken);

        //assert
        Assert.That(res1, Is.True);
        Assert.That(res2, Is.True);
        Assert.That(res3, Is.False);
    }

    /// <summary>
    /// Tests: For US-based tokens, we use X requests per timespan; for EU-based tokens, a certain timespan has passed since the last call.
    /// </summary>
    [Test]
    public void WinterCounterAndBasicTimeSpanLocationsSuccessTest1()
    {
        //arrange
        var rateLimiter = new RateLimiter();
        var resourceA = "/testA/";
        var rules = new List<Rule>();
        var ruleA = new Rule();
        ruleA.LocationFilter = new List<string>() { "US" };
        ruleA.RuleAlg = new WindowCounterRule(3, 3);
        var ruleB = new Rule();
        ruleB.LocationFilter = new List<string>() { "EU" };
        ruleB.RuleAlg = new BasicTimeSpanRule(TimeSpan.FromSeconds(1));

        rules.Add(ruleA);
        rules.Add(ruleB);

        rateLimiter.SetRules(resourceA, rules);

        var accessTokenUS = "US1234";
        var accessTokenEU = "EU1111";

        //act
        var res1 = rateLimiter.SendRequest(resourceA, accessTokenUS);
        var res2 = rateLimiter.SendRequest(resourceA, accessTokenEU);

        //assert
        Assert.That(res1, Is.True);
        Assert.That(res2, Is.True);
    }
}