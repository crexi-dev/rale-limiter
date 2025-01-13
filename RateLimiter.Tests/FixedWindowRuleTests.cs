using System;
using NUnit.Framework;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class FixedWindowRuleTests
    {
        [Test]
        public void FirstRequest_Should_BeAllowed()
        {
            var usageRepo = new InMemoryUsageRepository();

            // assume we only allow 1 request in a 1-minute window for this test
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var isAllowed = rule.IsRequestAllowed("crexi-client123");

            Assert.IsTrue(isAllowed, "first request from a new client should always be allowed.");

        }

        [Test]
        public void SecondRequest_WithinSameWindow_Should_BeBlocked()
        {

            var usageRepo = new InMemoryUsageRepository();

            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var firstRequestIsAllowed= rule.IsRequestAllowed("crexi-client123");
            var secondRequestIsAllowed = rule.IsRequestAllowed("crexi-client123");

            Assert.IsTrue(firstRequestIsAllowed, "first request should be allowed.");
            Assert.IsFalse(secondRequestIsAllowed, "second request should be blocked within the same tiem window.");
        }

        [Test]
        public void SecondRequest_WithinSameWindowButFromDifferentClient_Should_BeAllowed()
        {
            var usageRepo = new InMemoryUsageRepository();

            // second request from different client but within same time window
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var firstRequestIsAllowed = rule.IsRequestAllowed("crexi-client123");
            
            // this verifies that each client is tracked independently
            var secondRequestIsAllowed = rule.IsRequestAllowed("crexi-client456");

            Assert.IsTrue(firstRequestIsAllowed, "first request should be allowed.");
            Assert.IsTrue(secondRequestIsAllowed, "first request from different client should be allowed.");
        }

        [Test]
        public void ThirdRequest_WithinSameWindowButFromSameClient_Should_BeBlocked()
        {
            var usageRepo = new InMemoryUsageRepository();

            // second request from different client but within same time window
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var firstRequestIsAllowed = rule.IsRequestAllowed("crexi-client123");
            var secondRequestIsAllowed = rule.IsRequestAllowed("crexi-client456");

            // now ensure second client cannot break tiem window rule
            var thirdRequestIsAllowed = rule.IsRequestAllowed("crexi-client456");

            Assert.IsTrue(firstRequestIsAllowed, "first request should be allowed.");
            Assert.IsTrue(secondRequestIsAllowed, "first request from second client should be allowed.");

            // should be blocked
            Assert.IsFalse(thirdRequestIsAllowed, "second request from second client should not be allowed.");
        }
    }
}
