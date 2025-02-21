using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Stores;
using System.Collections.Generic;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void RateLimiter_AllowsRequest_When_NoRulesAreSet()
	{
        // Arrange
        var resourceId = "/api/resource";
        var userId = "user1";
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object);

        // Act
        var allowed = rateLimiter.IsRequestAllowed(resourceId, userId);

        // Assert
		Assert.That(allowed, Is.True);
	}

    [Test]
    public void RateLimiter_AllowsRequest_When_WithinConfiguredRuleLimits()
    {
        // Arrange
        var resourceId = "/api/resource";
        var userId = "user1";
        var testRule = new Mock<IRateLimitRule>();
        var ruleList = new List<IRateLimitRule>()
        {
            testRule.Object
        };
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object);

        testRule.Setup(testRule => testRule.IsWithinLimit(userId)).Returns(true);
        rulesetStoreMock.Setup(store => store.GetRules(resourceId)).Returns(ruleList);

        // Act
        var allowed = rateLimiter.IsRequestAllowed(resourceId, userId);

        // Assert
        Assert.That(allowed, Is.True);
    }

    [Test]
    public void RateLimiter_DeniesRequest_When_ConfiguredRulesFail()
    {
        // Arrange
        var resourceId = "/api/resource";
        var userId = "user1";
        var testRule1 = new Mock<IRateLimitRule>();
        var testRule2 = new Mock<IRateLimitRule>();
        var ruleList = new List<IRateLimitRule>()
        {
            testRule1.Object,
            testRule2.Object
        };
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object);

        testRule1.Setup(testRule => testRule.IsWithinLimit(userId)).Returns(true);
        testRule2.Setup(testRule => testRule.IsWithinLimit(userId)).Returns(false);
        rulesetStoreMock.Setup(store => store.GetRules(resourceId)).Returns(ruleList);

        // Act
        var allowed = rateLimiter.IsRequestAllowed(resourceId, userId);

        // Assert
        Assert.That(allowed, Is.False);
    }
}