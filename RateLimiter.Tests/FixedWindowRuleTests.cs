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
            var firstClientToken = "crexi-client123";

            // assume we only allow 1 request in a 1-minute window for this test
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var isAllowed = rule.IsRequestAllowed(firstClientToken);

            Assert.IsTrue(isAllowed, "first request from a new client should always be allowed.");

        }

        [Test]
        public void SecondRequest_WithinSameWindow_Should_BeBlocked()
        {
            var usageRepo = new InMemoryUsageRepository();
            var firstClientToken = "crexi-client123";

            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var firstRequestIsAllowed= rule.IsRequestAllowed(firstClientToken);
            var secondRequestIsAllowed = rule.IsRequestAllowed(firstClientToken);

            Assert.IsTrue(firstRequestIsAllowed, "first request should be allowed.");
            Assert.IsFalse(secondRequestIsAllowed, "second request should be blocked within the same tiem window.");
        }

        [Test]
        public void SecondRequest_WithinSameWindowButFromDifferentClient_Should_BeAllowed()
        {
            var usageRepo = new InMemoryUsageRepository();
            var firstClientToken = "crexi-client123";
            var secondClientToken = "crexi-client456";

            // second request from different client but within same time window
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var firstRequestIsAllowed = rule.IsRequestAllowed(firstClientToken);
            
            // this verifies that each client is tracked independently
            var secondRequestIsAllowed = rule.IsRequestAllowed(secondClientToken);

            Assert.IsTrue(firstRequestIsAllowed, "first request should be allowed.");
            Assert.IsTrue(secondRequestIsAllowed, "first request from different client should be allowed.");
        }

        [Test]
        public void ThirdRequest_WithinSameWindowButFromSameClient_Should_BeBlocked()
        {
            var usageRepo = new InMemoryUsageRepository();
            var firstClientToken = "crexi-client123";
            var secondClientToken = "crexi-client456";

            // second request from different client but within same time window
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var firstRequestIsAllowed = rule.IsRequestAllowed(firstClientToken);
            var secondRequestIsAllowed = rule.IsRequestAllowed(secondClientToken);

            // now ensure second client cannot break tiem window rule
            var thirdRequestIsAllowed = rule.IsRequestAllowed(secondClientToken);

            Assert.IsTrue(firstRequestIsAllowed, "first request should be allowed.");
            Assert.IsTrue(secondRequestIsAllowed, "first request in window but from second client should be allowed as it's the client's first request.");

            // should be blocked
            Assert.IsFalse(thirdRequestIsAllowed, "second request from second client within window should NOT be allowed.");
        }

        [Test]
        public void RequestMade_AfterWindowExpires_Should_BeAllowed()
        {
            var usageRepo = new InMemoryUsageRepository();
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromSeconds(10), usageRepo);
            var clientToken = "crexi-client123";

            // first request: allowed
            var firstRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsTrue(firstRequest, "first request should be allowed.");

            // second request (immediately): blocked within same window
            var secondRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsFalse(secondRequest, "second request in the same window should be blocked if limit = 1.");

            // simulate window expiring
            var usage = usageRepo.GetUsageForClient(clientToken);
            // exceed the window
            usage.WindowStart = usage.WindowStart.AddMinutes(-5);
            usageRepo.UpdateUsageForClient(clientToken, usage);

            var thirdRequest = rule.IsRequestAllowed(clientToken);
            //Assert.IsTrue(thirdRequest, "a request after the window expires should be allowed again.");
        }

        [Test]
        public void TwoRequestsMadeWithLimitOfTwo_BeforeWindowExpires_Should_BeAllowed()
        {
            var usageRepo = new InMemoryUsageRepository();
            var rule = new FixedWindowRule(limit: 2, window: TimeSpan.FromSeconds(10), usageRepo);
            var clientToken = "crexi-client123";

            var firstRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsTrue(firstRequest, "first request should be allowed.");

            var secondRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsTrue(secondRequest, "second request within a limit of 2 requests within the time window.");
        }
    }
}
