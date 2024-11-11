using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Services.Common.Configurations;
using Services.Common.RateLimitRules;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimitRuleFactoryTest
{
    private RateLimitRuleFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _factory = new RateLimitRuleFactory();
    }

    [Test]
    public void CreateRules_ShouldReturnRequestPerTimespanRule_WhenRuleTypeIsRequestPerTimespan()
    {
        // Arrange
        var config = new RuleConfig
        {
            RuleTypes = new List<string> { "RequestPerTimespan" },
            RequestLimit = 10,
            Timespan = TimeSpan.FromMinutes(1)
        };

        // Act
        var rules = _factory.CreateRules(config).ToList();

        // Assert
        Assert.AreEqual(1, rules.Count);
        Assert.IsInstanceOf<RequestPerTimespanRule>(rules.First());
        var rule = (RequestPerTimespanRule)rules.First();
    }

    [Test]
    public void CreateRules_ShouldReturnTimeSinceLastCallRule_WhenRuleTypeIsTimeSinceLastCall()
    {
        // Arrange
        var config = new RuleConfig
        {
            RuleTypes = new List<string> { "TimeSinceLastCall" },
            MinIntervalBetweenRequests = TimeSpan.FromSeconds(5)
        };

        // Act
        var rules = _factory.CreateRules(config).ToList();

        // Assert
        Assert.AreEqual(1, rules.Count);
        Assert.IsInstanceOf<TimeSinceLastCallRule>(rules.First());
        var rule = (TimeSinceLastCallRule)rules.First();
    }

    [Test]
    public void CreateRules_ShouldThrowArgumentException_WhenRuleTypeIsUnknown()
    {
        // Arrange
        var config = new RuleConfig
        {
            RuleTypes = new List<string> { "UnknownRuleType" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _factory.CreateRules(config).ToList());
        Assert.AreEqual("Unknown rule type: UnknownRuleType", exception.Message);
    }

    [Test]
    public void CreateRules_ShouldReturnMultipleRules_WhenMultipleRuleTypesAreConfigured()
    {
        // Arrange
        var config = new RuleConfig
        {
            RuleTypes = new List<string> { "RequestPerTimespan", "TimeSinceLastCall" },
            RequestLimit = 5,
            Timespan = TimeSpan.FromMinutes(1),
            MinIntervalBetweenRequests = TimeSpan.FromSeconds(10)
        };

        // Act
        var rules = _factory.CreateRules(config).ToList();

        // Assert
        Assert.AreEqual(2, rules.Count);
        Assert.IsInstanceOf<RequestPerTimespanRule>(rules[0]);
        Assert.IsInstanceOf<TimeSinceLastCallRule>(rules[1]);
    }
}