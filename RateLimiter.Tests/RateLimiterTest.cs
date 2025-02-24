using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;
using RateLimiter.Stores;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public async Task RateLimiter_AllowsRequest_When_NoRulesAreSet()
	{
        // Arrange
        var allowRequestsOnFailure = true;
        var resourceId = "/api/resource";
        var userId = "user1";
        var request = new RequestModel(resourceId, userId, string.Empty, string.Empty, string.Empty);
        var rulesetStoreMock = new Mock<IRulesetStore>();
        var loggerMock = new Mock<ILogger<RateLimiter>>();
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object, loggerMock.Object, allowRequestsOnFailure);

        // Act
        var allowed = await rateLimiter.IsRequestAllowedAsync(request);

        // Assert
		Assert.That(allowed, Is.True);
	}

    [Test]
    public async Task RateLimiter_AllowsRequest_When_WithinConfiguredRuleLimits()
    {
        // Arrange
        var allowRequestsOnFailure = true;
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
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object, loggerMock.Object, allowRequestsOnFailure);

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
        var allowRequestsOnFailure = true;
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
        var rateLimiter = new RateLimiter(rulesetStoreMock.Object, loggerMock.Object, allowRequestsOnFailure);

        testRule1.Setup(testRule => testRule.IsWithinLimitAsync(request)).ReturnsAsync(true);
        testRule2.Setup(testRule => testRule.IsWithinLimitAsync(request)).ReturnsAsync(false);
        rulesetStoreMock.Setup(store => store.GetRules(requestPath)).Returns(ruleList);

        // Act
        var allowed = await rateLimiter.IsRequestAllowedAsync(request);

        // Assert
        Assert.That(allowed, Is.False);
    }
}