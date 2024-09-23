using NUnit.Framework;
using RateLimiter.RateLimiters;
using System;

namespace RateLimiter.Tests
{
    [Obsolete("Static rule.")]
    [TestFixture]
    public class SlidingWindowRateLimiterTest
    {
        [Test]
        public void Should_AllowRequests_WithinLimit()
        {
            var rateLimiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
            var clientId = "client1";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }
        }

        [Test]
        public void Should_DenyRequests_ExceedingLimit()
        {
            var rateLimiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
            var clientId = "client2";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }

            // Sixth request should be denied
            Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_ResetLimit_GraduallyInSlidingWindow()
        {
            var rateLimiter = new SlidingWindowRateLimiter(5, TimeSpan.FromSeconds(5));
            var clientId = "client3";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
                System.Threading.Thread.Sleep(1000); // Spread out the requests within the window
            }

            // At this point, some requests have aged out, allowing new ones
            Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
        }
    }
}
