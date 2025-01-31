using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using RateLimiter.Limiter;
using RateLimiter.Storage;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class RateLimiterTest
{
    [Test]
    public async Task ApplyRateLimitRules_ReturnsIsAllowedFalse_WhenLimitsExceeded()
    {
        //Arrange
        var anyResource = "testResource";
        
        var storage = new DefaultRateLimiterStorage<string>();
        
        var slidingRuleRateLimiter = new RuleRateLimiter<string, string>(
            resource =>
            {
                var key = resource;

                var factory = new RateLimitRuleByKeyFactory<string>
                {
                    Key = key,
                    LimitRule = _ => new SlidingWindowRule(2, TimeSpan.FromSeconds(3))
                };

                return factory;
            }, storage
        );

        var windowRuleRateLimiter = new RuleRateLimiter<string, string>(
            resource =>
            {
                var key = resource;

                var factory = new RateLimitRuleByKeyFactory<string>
                {
                    Key = key,
                    LimitRule = _ => new FixedWindowRule(3, TimeSpan.FromSeconds(5))
                };

                return factory;
            }, storage
        );

        var list = new List<RuleRateLimiter<string, string>>
        {
            slidingRuleRateLimiter,
            windowRuleRateLimiter
        };

        var sut = new RateLimiter<string, string>(list);

        //Act
        var rateLimitResult1 = await sut.ApplyRateLimitRulesAsync(anyResource);
        var rateLimitResult2 = await sut.ApplyRateLimitRulesAsync(anyResource);
        var rateLimitResult3 = await sut.ApplyRateLimitRulesAsync(anyResource);
        var rateLimitResult4 = await sut.ApplyRateLimitRulesAsync(anyResource);

        await Task.Delay(TimeSpan.FromSeconds(7));

        var rateLimitResult5 = await sut.ApplyRateLimitRulesAsync(anyResource);

        //Assert
        Assert.That(rateLimitResult1.IsAllowed);
        Assert.That(rateLimitResult1.RulesMessages, Is.All.Null);

        Assert.That(rateLimitResult2.IsAllowed);
        Assert.That(rateLimitResult2.RulesMessages, Is.All.Null);

        Assert.That(!rateLimitResult3.IsAllowed);
        Assert.That(rateLimitResult3.RulesMessages.Count(x => x != null) == 1);

        Assert.That(!rateLimitResult4.IsAllowed);
        Assert.That(rateLimitResult4.RulesMessages.Count(x => x != null) == 2);

        Assert.That(rateLimitResult5.IsAllowed);
        Assert.That(rateLimitResult5.RulesMessages, Is.All.Null);
    }

    [Test]
    public async Task ApplyRateLimitRules_ReturnsIsAllowedTrue_WhenTheRulesWereNotConfigured()
    {
        //Arrange
        var anyResource = "testResource";

        var sut = new RateLimiter<string, string>(new List<RuleRateLimiter<string, string>>());

        //Act
        var rateLimitResult1 = await sut.ApplyRateLimitRulesAsync(anyResource);

        //Assert
        Assert.That(rateLimitResult1.IsAllowed);
        CollectionAssert.IsEmpty(rateLimitResult1.RulesMessages);
    }
}