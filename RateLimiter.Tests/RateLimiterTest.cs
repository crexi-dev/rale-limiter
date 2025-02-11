using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Services.Interfaces;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    Mock<ILogger<RateLimiter>> _mockLogger;
    Mock<IRuleService> _mockRuleService;
    IRateLimiter _subject;

    public RateLimiterTest()
	{
        _mockLogger = new Mock<ILogger<RateLimiter>>();
        _mockRuleService = new Mock<IRuleService>();

        _subject = new RateLimiter(_mockLogger.Object, _mockRuleService.Object);
	}

	[Test]
	public async Task RateLimiterTest_MockedServiceResponse_ShouldNotAllowRequest()
	{
        // Arrange
        var resourceAPI = "www.myapi.com/user";
        var authToken = "75938394-f733-4b88-9741-961b5f71b815";

        _mockRuleService.Setup(x => x.HasRateLimitExceeded(resourceAPI, authToken))
           .ReturnsAsync(true);

        // Act
        var limitExceed = await _subject.IsRequestAllowed(resourceAPI, authToken);

		// Assert
		Assert.That(limitExceed, Is.False);
	}

    [Test]
    public async Task RateLimiterTest_MockedServiceResponse_ShouldAllowRequest()
    {
        // Arrange
        var resource = "www.myapi.com/customer";
        var token = $"{Guid.NewGuid()}";

        _mockRuleService.Setup(x => x.HasRateLimitExceeded(resource, token))
           .ReturnsAsync(false);

        // Act
        var limitExceed = await _subject.IsRequestAllowed(resource, token);

        // Assert 
        Assert.That(limitExceed, Is.True);
    }
}