using Moq;
using RateLimitingLibrary.Config;
using RateLimitingLibrary.Core.Interfaces;
using RateLimitingLibrary.Core.Models;
using RateLimitingLibrary.Core.Services;
using RateLimitingLibrary.Rules;
using RateLimitingTests.Configurations;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RateLimitingTests.Services
{
    public class RateLimiterTests
    {
        [Fact]
        public async Task EvaluateRequest_WithinLimit_ReturnsAllowed()
        {
            // Arrange
            var fixedWindowMock = new Mock<IRateLimitRule>();
            var slidingWindowMock = new Mock<IRateLimitRule>();

            fixedWindowMock
                .Setup(r => r.Evaluate(It.IsAny<ClientRequest>()))
                .Returns(new RateLimitResult { IsAllowed = true });

            var config = new RuleConfigurations
            {
                ResourceRules = new()
                {
                    { "ResourceA", new() { fixedWindowMock.Object.GetType().Name } }
                }
            };

            var rateLimiter = new RateLimiter(config, new[] { fixedWindowMock.Object, slidingWindowMock.Object });
            var request = MockConfiguration.CreateRequest("Client1", "ResourceA", DateTime.UtcNow);

            // Act
            var result = await rateLimiter.EvaluateRequestAsync(request);

            // Assert
            Assert.True(result.IsAllowed);
            fixedWindowMock.Verify(r => r.Evaluate(It.IsAny<ClientRequest>()), Times.Once);
            slidingWindowMock.Verify(r => r.Evaluate(It.IsAny<ClientRequest>()), Times.Never);
        }

        [Fact]
        public async Task EvaluateRequest_ExceedsLimit_ReturnsDenied()
        {
            // Arrange
            var fixedWindowMock = new Mock<IRateLimitRule>();
            var slidingWindowMock = new Mock<IRateLimitRule>();

            fixedWindowMock
                .Setup(r => r.Evaluate(It.IsAny<ClientRequest>()))
                .Returns(new RateLimitResult { IsAllowed = false });

            var config = new RuleConfigurations
            {
                ResourceRules = new()
                {
                    { "ResourceA", new() { fixedWindowMock.Object.GetType().Name } }
                }
            };

            var rateLimiter = new RateLimiter(config, new[] { fixedWindowMock.Object, slidingWindowMock.Object });
            var request = MockConfiguration.CreateRequest("Client1", "ResourceA", DateTime.UtcNow);

            // Act
            var result = await rateLimiter.EvaluateRequestAsync(request);

            // Assert
            Assert.False(result.IsAllowed);
            fixedWindowMock.Verify(r => r.Evaluate(It.IsAny<ClientRequest>()), Times.Once);
            slidingWindowMock.Verify(r => r.Evaluate(It.IsAny<ClientRequest>()), Times.Never);
        }

        [Fact]
        public async Task EvaluateRequest_MultipleRules_AllowsRequestIfAllPass()
        {
            // Arrange
            var fixedWindowMock = new Mock<IRateLimitRule>();
            var slidingWindowMock = new Mock<IRateLimitRule>();

            fixedWindowMock
                .Setup(r => r.Evaluate(It.IsAny<ClientRequest>()))
                .Returns(new RateLimitResult { IsAllowed = true });

            slidingWindowMock
                .Setup(r => r.Evaluate(It.IsAny<ClientRequest>()))
                .Returns(new RateLimitResult { IsAllowed = true });

            var config = new RuleConfigurations
            {
                ResourceRules = new()
                {
                    { "ResourceB", new() { fixedWindowMock.Object.GetType().Name, slidingWindowMock.Object.GetType().Name } }
                }
            };

            var rateLimiter = new RateLimiter(config, new[] { fixedWindowMock.Object, slidingWindowMock.Object });
            var request = MockConfiguration.CreateRequest("Client1", "ResourceB", DateTime.UtcNow);

            // Act
            var result = await rateLimiter.EvaluateRequestAsync(request);

            // Assert
            Assert.True(result.IsAllowed);
        }

        [Fact]
        public async Task EvaluateRequest_MultipleRules_DeniesRequestIfAnyRuleFails()
        {
            // Arrange
            var fixedWindowMock = new Mock<IRateLimitRule>();
            var slidingWindowMock = new Mock<IRateLimitRule>();

            fixedWindowMock
                .Setup(r => r.Evaluate(It.IsAny<ClientRequest>()))
                .Returns(new RateLimitResult { IsAllowed = false }); // First rule passes

            slidingWindowMock
                .Setup(r => r.Evaluate(It.IsAny<ClientRequest>()))
                .Returns(new RateLimitResult { IsAllowed = false }); // Second rule fails

            var config = new RuleConfigurations
            {
                ResourceRules = new()
                {
                    { "ResourceB", new() { fixedWindowMock.Object.GetType().Name, slidingWindowMock.Object.GetType().Name } }
                }
            };

            var rateLimiter = new RateLimiter(config, new[] { fixedWindowMock.Object, slidingWindowMock.Object });
            var request = MockConfiguration.CreateRequest("Client1", "ResourceB", DateTime.UtcNow);

            // Act
            var result = await rateLimiter.EvaluateRequestAsync(request);

            // Assert
            Assert.False(result.IsAllowed);

            // Verify FixedWindowRule was invoked exactly once
            fixedWindowMock.Verify(r => r.Evaluate(It.IsAny<ClientRequest>()), Times.Once);

            // Verify SlidingWindowRule was invoked exactly once and caused denial
            slidingWindowMock.Verify(r => r.Evaluate(It.IsAny<ClientRequest>()), Times.Never);
        }
    }
}