using System;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Tests
{
    [TestFixture]
    internal class CooldownRuleTests
    {
        [Test]
        public void FirstRequest_Should_BeAllowed()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var rule = new CooldownRule(usageRepo, TimeSpan.FromSeconds(5));
            var clientToken = "crexi-client123";

            bool isAllowed = rule.IsRequestAllowed(clientToken);

            Assert.IsTrue(isAllowed, "first request should always be allowed since there's no prior usage.");
        }

        [Test]
        public void SecondRequest_WithinCooldown_Should_BeBlocked()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var rule = new CooldownRule(usageRepo, TimeSpan.FromSeconds(5));
            var clientToken = "crexi-client123";

            bool firstRequest = rule.IsRequestAllowed(clientToken);
            bool secondRequest = rule.IsRequestAllowed(clientToken);

            Assert.IsTrue(firstRequest, "first request should be allowed.");
            Assert.IsFalse(secondRequest, "second request within 5 seconds should be blocked.");
        }

        [Test]
        public void Request_AfterCooldown_Should_BeAllowed()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var rule = new CooldownRule(usageRepo, TimeSpan.FromSeconds(5));
            var clientToken = "crexi-client123";

            bool firstRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsTrue(firstRequest);

            bool secondRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsFalse(secondRequest);

            // simulate time passing
            var usage = usageRepo.GetUsageForClient(clientToken);
            usage.LastRequestTime = usage.LastRequestTime.AddSeconds(-6);
            usageRepo.UpdateUsageForClient(clientToken, usage);

            var thirdRequest = rule.IsRequestAllowed(clientToken);
            Assert.IsTrue(thirdRequest, "a request after cooldown should be allowed.");
        }
    }
}
