using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.RateLimiters;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class ResourceRateLimiterTest
    {
        [Test]
        public void Should_AllowRequests_WithinLimit_US()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
            {
                new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1))
            });

            var clientId = "usClient";
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }
        }

        [Test]
        public void Should_DenyRequests_ExceedingLimit_US()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
            {
                new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1))
            });

            var clientId = "usClient";
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }

            // Sixth request should be denied
            Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_AllowRequests_EU_RuleAfterTimeWindow()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
            {
                new TimespanSinceLastCallRule(TimeSpan.FromSeconds(2))
            });

            var clientId = "euClient";
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));

            // Within 2 seconds, should be denied
            Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));

            // Wait for 2 seconds, should be allowed again
            System.Threading.Thread.Sleep(2100);
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_DenyRequests_EU_RuleWithinTimeWindow()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
            {
                new TimespanSinceLastCallRule(TimeSpan.FromSeconds(2))
            });

            var clientId = "euClient";
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));

            // Immediately requesting again should be denied
            Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_ApplyMultipleRules()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
            {
                new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1)),
                new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1)) // Delay added for testing purposes
            });

            var clientId = "multiRuleClient";

            // First 5 requests should be allowed
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
                System.Threading.Thread.Sleep(1100); // Ensure we respect TimespanSinceLastCallRule
            }

            // Sixth request should still be denied due to the request count rule
            Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));

            // Wait for the XRequestsPerTimespanRule to reset
            System.Threading.Thread.Sleep(61000);

            // After the reset, requests should be allowed again
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
        }

    }
}
