using NUnit.Framework;
using Moq;
using RateLimiter.Rules;
using System.Collections.Generic;
using RateLimiter.Configuration;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTests
{
    private Mock<IRateLimiterConfiguration> _mockConfiguration;
    private Mock<IRateLimiterRule> _mockRule;
    private RateLimiter _rateLimiter;

    [SetUp]
    public void SetUp()
    {
        _mockConfiguration = new Mock<IRateLimiterConfiguration>();
        _mockRule = new Mock<IRateLimiterRule>();
        _rateLimiter = new RateLimiter(_mockConfiguration.Object);
    }

    [Test]
    public void IsAllowed_WhenNoRules_ReturnsTrue()
    {
        // Arrange
        string clientToken = "client1";
        string uri = "https://api.example.com/resource";
        _mockConfiguration.Setup(c => c.GetRules(uri)).Returns((IEnumerable<IRateLimiterRule>)null);

        // Act
        var result = _rateLimiter.IsAllowed(clientToken, uri);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsAllowed_WhenRulesExistAndRuleIsAllowed_ReturnsTrue()
    {
        // Arrange
        string clientToken = "client1";
        string uri = "https://api.example.com/resource";
        var rules = new List<IRateLimiterRule> { _mockRule.Object };
        _mockConfiguration.Setup(c => c.GetRules(uri)).Returns(rules);
        _mockRule.Setup(r => r.IsAllowed(clientToken, uri)).Returns(true);

        // Act
        var result = _rateLimiter.IsAllowed(clientToken, uri);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsAllowed_WhenRulesExistAndRuleIsNotAllowed_ReturnsFalse()
    {
        // Arrange
        string clientToken = "client1";
        string uri = "https://api.example.com/resource";
        var rules = new List<IRateLimiterRule> { _mockRule.Object };
        _mockConfiguration.Setup(c => c.GetRules(uri)).Returns(rules);
        _mockRule.Setup(r => r.IsAllowed(clientToken, uri)).Returns(false);

        // Act
        var result = _rateLimiter.IsAllowed(clientToken, uri);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void IsAllowed_WhenMultipleRulesExist_AllRulesMustAllow_ReturnsFalseWhenOneRuleDisallows()
    {
        // Arrange
        string clientToken = "client1";
        string uri = "https://api.example.com/resource";
        var rules = new List<IRateLimiterRule>
            {
                _mockRule.Object,
                _mockRule.Object
            };
        _mockConfiguration.Setup(c => c.GetRules(uri)).Returns(rules);

        _mockRule.Setup(r => r.IsAllowed(clientToken, uri)).Returns(true);
        _mockRule.Setup(r => r.IsAllowed(clientToken, uri)).Returns(false);

        // Act
        var result = _rateLimiter.IsAllowed(clientToken, uri);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void IsAllowed_WhenMultipleRulesExist_AllMustAllow_ReturnsTrueWhenAllRulesAllow()
    {
        // Arrange
        string clientToken = "client1";
        string uri = "https://api.example.com/resource";
        var rules = new List<IRateLimiterRule>
            {
                _mockRule.Object,
                _mockRule.Object
            };
        _mockConfiguration.Setup(c => c.GetRules(uri)).Returns(rules);

        _mockRule.Setup(r => r.IsAllowed(clientToken, uri)).Returns(true);

        // Act
        var result = _rateLimiter.IsAllowed(clientToken, uri);

        // Assert
        Assert.IsTrue(result);
    }
}