
using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimitRuleTests
    {
        [Test]
        public void Should_AllowRequestsWithinLimit()
        {
            var rule = new XRequestsPerTimespanRule(3, TimeSpan.FromSeconds(10));
            var clientId = "test-client";
            var resource = "test-resource";


            Assert.That(rule.IsRequestAllowed(clientId, resource), Is.True, "The request should be allowed.");
            Assert.That(rule.IsRequestAllowed(clientId, resource), Is.True, "The request should be allowed.");
            Assert.That(rule.IsRequestAllowed(clientId, resource), Is.True, "The request should be allowed.");
            Assert.That(rule.IsRequestAllowed(clientId, resource), Is.False, "The request should not be allowed.");

        }

        [Test]
        public void ClientRateLimit_AllowsWithinLimit()
        {
            var rule = new ClientRateLimitRule(5, TimeSpan.FromMinutes(1));
            var clientId = "client1";

            for (int i = 0; i < 5; i++)
            {
                Assert.That(rule.IsRequestAllowed(clientId, "resource1", "127.0.0.1"), Is.True, "Request should be allowed within the limit.");
            }

            Assert.That(rule.IsRequestAllowed(clientId, "resource1", "127.0.0.1"), Is.False, "Request should be denied after exceeding the limit.");
        }

        [Test]
        public void ResourceRateLimit_AllowsWithinLimit()
        {
            var resourceLimits = new Dictionary<string, int> { { "resource1", 2 } };
            var rule = new ResourceRateLimitRule(resourceLimits);

            Assert.That(rule.IsRequestAllowed("client1", "resource1", "127.0.0.1"), Is.True);
            Assert.That(rule.IsRequestAllowed("client1", "resource1", "127.0.0.1"), Is.True);
            Assert.That(rule.IsRequestAllowed("client1", "resource1", "127.0.0.1"), Is.False);
        }


    }
}
