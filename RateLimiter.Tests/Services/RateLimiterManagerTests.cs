using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services.Rules;
using RateLimiter.Services;
using System.Collections.Generic;
using System;

namespace RateLimiter.Tests.Services
{
    public class RateLimiterManagerTests
    {
        private RateLimiterManager _manager;

        private const string TEST_RESOURCE = "/api/test/resource";
        private const string DIFFERENT_RESOURCE = "/different/resource";
        private const string CLIENT_ID = "TestClient";
        private const string UNKNOWN_CLIENT = "UnknownClient";

        private const int HTTP_STATUS_OK = 200;
        private const int RATE_LIMIT_COUNT = 1;
        private static readonly TimeSpan RATE_LIMIT_DURATION = TimeSpan.FromSeconds(5);

        [SetUp]
        public void Setup()
        {
            _manager = new RateLimiterManager();
        }

        [Test]
        public void GivenUnknownClient_WhenRequestMade_ReturnsFalse()
        {
            var result = _manager.IsRequestAllowed(UNKNOWN_CLIENT, TEST_RESOURCE);
            Assert.That(result.IsAllowed, Is.False);
        }

        [Test]
        public void GivenUnconfiguredResource_WhenRequestMade_ReturnsFalse()
        {
            var config = CreateTestConfig(CLIENT_ID, TEST_RESOURCE);
            _manager.AddRateLimitRules(new List<ClientRateLimitConfig> { config });

            var result = _manager.IsRequestAllowed(CLIENT_ID, DIFFERENT_RESOURCE);
            Assert.That(result.IsAllowed, Is.False);
        }

        [Test]
        public void GivenValidConfiguration_WhenRequestMade_AllowsRequests()
        {
            var config = CreateTestConfig(CLIENT_ID, TEST_RESOURCE);
            _manager.AddRateLimitRules(new List<ClientRateLimitConfig> { config });

            var result = _manager.IsRequestAllowed(CLIENT_ID, TEST_RESOURCE);
            Assert.That(result.IsAllowed, Is.True);
        }

        [Test]
        public void GivenDuplicateClientConfig_WhenAdded_ThrowsException()
        {
            var config = CreateTestConfig(CLIENT_ID, TEST_RESOURCE);
            _manager.AddRateLimitRules(new List<ClientRateLimitConfig> { config });

            Assert.Throws<ArgumentException>(() =>
                _manager.AddRateLimitRules(new List<ClientRateLimitConfig> { config }));
        }

        [Test]
        public void GivenMultipleRules_WhenOneFails_RequestIsDenied()
        {
            var config = new ClientRateLimitConfig
            {
                ClientId = CLIENT_ID,
                ResourceLimits = new List<ResourceRateLimitConfig>
                {
                    new ResourceRateLimitConfig
                    {
                        Resource = TEST_RESOURCE,
                        Rules = new List<IRateLimitRule>
                        {
                            new FixedWindowRateLimit(RATE_LIMIT_COUNT, RATE_LIMIT_DURATION),
                            new SlidingWindowRateLimit(RATE_LIMIT_COUNT, RATE_LIMIT_DURATION)
                        }
                    }
                }
            };

            _manager.AddRateLimitRules(new List<ClientRateLimitConfig> { config });

            // First request should pass both rules
            Assert.That(_manager.IsRequestAllowed(CLIENT_ID, TEST_RESOURCE).IsAllowed, Is.True);

            // Second request should fail both rules
            var result = _manager.IsRequestAllowed(CLIENT_ID, TEST_RESOURCE);
            Assert.That(result.IsAllowed, Is.False);
        }

        private static ClientRateLimitConfig CreateTestConfig(string clientId, string resource)
        {
            return new ClientRateLimitConfig
            {
                ClientId = clientId,
                ResourceLimits = new List<ResourceRateLimitConfig>
                {
                    new ResourceRateLimitConfig
                    {
                        Resource = resource,
                        Rules = new List<IRateLimitRule>
                        {
                            new FixedWindowRateLimit(RATE_LIMIT_COUNT, RATE_LIMIT_DURATION)
                        }
                    }
                }
            };
        }
    }
}
