using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Services.Common.Configurations;
using Services.Common.Models;
using Services.Common.RateLimiters;
using Services.Common.RateLimitRules;
using Services.Common.Repositories;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private DynamicRateLimiter _rateLimiter;
    private Mock<IRuleRepository> _mockRuleRepository;
    private Mock<IRateLimitRuleFactory> _mockRuleFactory;
    private Mock<IRuleConfigLoader> _mockConfigLoader;
    private Mock<IRateLimitRule> _mockRule1;
    private Mock<IRateLimitRule> _mockRule2;

    [SetUp]
    public async Task SetUp()
    {
        _mockRuleRepository = new Mock<IRuleRepository>();
        _mockRuleFactory = new Mock<IRateLimitRuleFactory>();
        _mockConfigLoader = new Mock<IRuleConfigLoader>();
        _mockRule1 = new Mock<IRateLimitRule>();
        _mockRule2 = new Mock<IRateLimitRule>();
        _rateLimiter = new DynamicRateLimiter(_mockConfigLoader.Object);
    }

    [Test]
    public void IsRequestAllowed_ShouldReturnTrue_WhenAllRulesAllowRequest()
    {
        // Arrange
        var token = new RateLimitToken { Id = Guid.NewGuid(), Resource = "api1", Region = "us-west" };

        // Set up mocks to return true for all rules
        _mockRule1.Setup(r => r.IsRequestAllowed(token.Id)).Returns(true);
        _mockRule2.Setup(r => r.IsRequestAllowed(token.Id)).Returns(true);

        // Configure the mock config loader to return both rules
        _mockConfigLoader.Setup(c => c.GetRulesForResource(token.Resource, token.Region))
            .Returns(new List<IRateLimitRule> { _mockRule1.Object, _mockRule2.Object });

        // Act
        var result = _rateLimiter.IsRequestAllowed(token);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsRequestAllowed_ShouldReturnFalse_WhenAnyRuleDeniesRequest()
    {
        // Arrange
        var token = new RateLimitToken { Id = Guid.NewGuid(), Resource = "api2", Region = "us-east" };

        // Set up mocks: one rule allows, one denies
        _mockRule1.Setup(r => r.IsRequestAllowed(token.Id)).Returns(true);
        _mockRule2.Setup(r => r.IsRequestAllowed(token.Id)).Returns(false);

        // Configure the mock config loader to return both rules
        _mockConfigLoader.Setup(c => c.GetRulesForResource(token.Resource, token.Region))
            .Returns(new List<IRateLimitRule> { _mockRule1.Object, _mockRule2.Object });

        // Act
        var result = _rateLimiter.IsRequestAllowed(token);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void IsRequestAllowed_ShouldReturnTrue_WhenNoRulesExistForResource()
    {
        // Arrange
        var token = new RateLimitToken { Id = Guid.NewGuid(), Resource = "api3", Region = "eu-central" };

        // Configure the mock config loader to return an empty rule list
        _mockConfigLoader.Setup(c => c.GetRulesForResource(token.Resource, token.Region))
            .Returns(new List<IRateLimitRule>());

        // Act
        var result = _rateLimiter.IsRequestAllowed(token);

        // Assert
        Assert.IsTrue(result);
    }
}