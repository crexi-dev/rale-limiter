using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;
using RateLimiter.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public async Task RateLimiter_AllowsRequest_When_NoRulesAreSet()
	{
        // Arrange
        var resourceId = "/api/resource";
        var userId = "user1";
        var request = new RequestModel(resourceId, userId, string.Empty, string.Empty, string.Empty);
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var loggerMock = new Mock<ILogger<RateLimiter>>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object, loggerMock.Object);

        // Act
        var allowed = await rateLimiter.IsRequestAllowedAsync(request);

        // Assert
		Assert.That(allowed, Is.True);
	}

    [Test]
    public async Task RateLimiter_AllowsRequest_When_WithinConfiguredRuleLimits()
    {
        // Arrange
        var resourceId = "/api/resource";
        var userId = "user1";
        var request = new RequestModel(resourceId, userId, string.Empty, string.Empty, string.Empty);
        var testRule = new Mock<IRateLimitRule>();
        var ruleList = new List<IRateLimitRule>()
        {
            testRule.Object
        };
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var loggerMock = new Mock<ILogger<RateLimiter>>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object, loggerMock.Object);

        testRule.Setup(testRule => testRule.IsWithinLimitAsync(request)).ReturnsAsync(true);
        rulesetStoreMock.Setup(store => store.GetRules(resourceId)).Returns(ruleList);

        // Act
        var allowed = await rateLimiter.IsRequestAllowedAsync(request);

        // Assert
        Assert.That(allowed, Is.True);
    }

    [Test]
    public async Task RateLimiter_DeniesRequest_When_ConfiguredRulesFail()
    {
        // Arrange
        var requestPath = "/api/resource";
        var userId = "user1";
        var request = new RequestModel(requestPath, userId, string.Empty, string.Empty, string.Empty);
        var testRule1 = new Mock<IRateLimitRule>();
        var testRule2 = new Mock<IRateLimitRule>();
        var ruleList = new List<IRateLimitRule>()
        {
            testRule1.Object,
            testRule2.Object
        };
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var loggerMock = new Mock<ILogger<RateLimiter>>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object, loggerMock.Object);

        testRule1.Setup(testRule => testRule.IsWithinLimitAsync(request)).ReturnsAsync(true);
        testRule2.Setup(testRule => testRule.IsWithinLimitAsync(request)).ReturnsAsync(false);
        rulesetStoreMock.Setup(store => store.GetRules(requestPath)).Returns(ruleList);

        // Act
        var allowed = await rateLimiter.IsRequestAllowedAsync(request);

        // Assert
        Assert.That(allowed, Is.False);
    }
}