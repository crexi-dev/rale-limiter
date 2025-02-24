using NUnit.Framework;
using System;
using System.Threading;

namespace RateLimiter.Tests.Services.Rules
{
    public class FixedWindowRateLimitTests
    {
        private FixedWindowRateLimit _rateLimit;
        private const string CLIENT_ID = "TestClient";

        [SetUp]
        public void Setup()
        {
            _rateLimit = new FixedWindowRateLimit(3, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void WhenFirstRequest_IsAllowed()
        {
            var result = _rateLimit.IsRequestAllowed(CLIENT_ID);
            Assert.That(result.IsAllowed, Is.True);
        }

        [Test]
        public void WhenUnderLimit_AllRequestsAllowed()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_rateLimit.IsRequestAllowed(CLIENT_ID).IsAllowed, Is.True);
                Assert.That(_rateLimit.IsRequestAllowed(CLIENT_ID).IsAllowed, Is.True);
                Assert.That(_rateLimit.IsRequestAllowed(CLIENT_ID).IsAllowed, Is.True);
            });
        }

        [Test]
        public void WhenOverLimit_RequestsBlocked()
        {
            // Use up the limit
            for (int i = 0; i < 3; i++)
            {
                _rateLimit.IsRequestAllowed(CLIENT_ID);
            }

            var result = _rateLimit.IsRequestAllowed(CLIENT_ID);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsAllowed, Is.False);
                Assert.That(result.RetryAfter, Is.GreaterThan(TimeSpan.Zero));
            });
        }

        [Test]
        public void WhenWindowExpires_AllowsNewRequests()
        {
            // Use up the limit
            for (int i = 0; i < 3; i++)
            {
                _rateLimit.IsRequestAllowed(CLIENT_ID);
            }

            // Wait for window to expire
            Thread.Sleep(5100);

            var result = _rateLimit.IsRequestAllowed(CLIENT_ID);
            Assert.That(result.IsAllowed, Is.True);
        }

 
    }
}
