using NUnit.Framework;
using RateLimiter.RateLimiters;
using System;

namespace RateLimiter.Tests
{
    [Obsolete("Static rule.")]
    [TestFixture]
    public class FixedWindowRateLimiterTest
    {
        [Test]
        public void Should_AllowRequests_WithinLimit()
        {
            var rateLimiter = new FixedWindowRateLimiter(5, TimeSpan.FromMinutes(1));
            var clientId = "client1";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }
        }

        [Test]
        public void Should_DenyRequests_ExceedingLimit()
        {
            var rateLimiter = new FixedWindowRateLimiter(5, TimeSpan.FromMinutes(1));
            var clientId = "client2";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }

            // Sixth request should be denied
            Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_ResetLimit_AfterTimeWindow()
        {
            var rateLimiter = new FixedWindowRateLimiter(5, TimeSpan.FromSeconds(1));
            var clientId = "client3";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }

            // Wait for time window to reset
            System.Threading.Thread.Sleep(1100);

            // New request after time window should be allowed
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
        }
    }
}
