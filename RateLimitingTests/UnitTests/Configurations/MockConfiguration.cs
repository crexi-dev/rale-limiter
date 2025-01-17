using Moq;
using RateLimitingLibrary.Config;
using RateLimitingLibrary.Core.Interfaces;
using RateLimitingLibrary.Core.Models;
using RateLimitingLibrary.Core.Services;
using System;
using System.Collections.Generic;

namespace RateLimitingTests.Configurations
{
    public static class MockConfiguration
    {
        public static IRateLimiter CreateRateLimiterWithMocks(out Mock<IRateLimitRule> fixedWindowMock, out Mock<IRateLimitRule> slidingWindowMock)
        {
            // Create mocks for rules
            fixedWindowMock = new Mock<IRateLimitRule>();
            slidingWindowMock = new Mock<IRateLimitRule>();

            // Configure mocks to return specific results
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
                    { "ResourceA", new() { "MockedFixedWindowRule" } },
                    { "ResourceB", new() { "MockedSlidingWindowRule" } }
                }
            };

            var rules = new List<IRateLimitRule>
            {
                fixedWindowMock.Object,
                slidingWindowMock.Object
            };

            return new RateLimiter(config, rules);
        }

        public static ClientRequest CreateRequest(string clientToken, string resource, DateTime requestTime)
        {
            return new ClientRequest
            {
                ClientToken = clientToken,
                Resource = resource,
                RequestTime = requestTime
            };
        }
    }
}