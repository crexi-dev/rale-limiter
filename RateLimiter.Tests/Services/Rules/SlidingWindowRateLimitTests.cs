using NUnit.Framework;
using RateLimiter.Services.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Services.Rules
{
    public class SlidingWindowRateLimitTests
    {
        private SlidingWindowRateLimit _rateLimit;
        private const string CLIENT_ID = "ClientA";

        [SetUp]
        public void Setup()
        {
            _rateLimit = new SlidingWindowRateLimit(3, TimeSpan.FromSeconds(5));
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
        public void WhenOldRequestsExpire_AllowsNewRequests()
        {
            // Make initial requests
            for (int i = 0; i < 3; i++)
            {
                _rateLimit.IsRequestAllowed(CLIENT_ID);
            }

            // Wait for oldest request to expire
            Thread.Sleep(5100);

            var result = _rateLimit.IsRequestAllowed(CLIENT_ID);
            Assert.That(result.IsAllowed, Is.True);
        }

    }
}
