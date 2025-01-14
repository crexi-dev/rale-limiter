using System;
using System.Diagnostics;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Tests
{
    [TestFixture]
    internal class CompositeRateLimitStrategyTests
    {
        [Test]
        public void Request_WithBothWindowAndCooldown_Should_BeBlocked_IfEitherFails()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var clientToken = "crexi-client123";

            // max 3 requestse per min
            var fixedWindowRule = new FixedWindowRule(limit: 3, window: TimeSpan.FromMinutes(1), usageRepo);
            // must wait 2 seconds between calls
            var cooldownRule = new CooldownRule(usageRepo, TimeSpan.FromSeconds(2));

            var composite = new CompositeRateLimitStrategy(new IRateLimitStrategy[] { fixedWindowRule, cooldownRule });

            bool firstRequest = composite.IsRequestAllowed(clientToken);
            bool secondRequest = composite.IsRequestAllowed(clientToken);

            Assert.IsTrue(firstRequest, "first request should pass both rules.");
            Assert.IsFalse(secondRequest, "second request fails cos the cooldown rule hasn't expired yet, even though fixed window limit isn't reached yet.");
        }

        [Test]
        public void Request_WithinFixedWindow_OK_IfCooldownIsSatisfied()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var clientToken = "crexi-client123";

            var fixedWindowRule = new FixedWindowRule(limit: 3, window: TimeSpan.FromMinutes(1), usageRepo);
            var cooldownRule = new CooldownRule(usageRepo, TimeSpan.FromSeconds(2));
            var composite = new CompositeRateLimitStrategy(new IRateLimitStrategy[] { fixedWindowRule, cooldownRule });

            bool firstRequest = composite.IsRequestAllowed(clientToken);
            Assert.IsTrue(firstRequest, "should pass if no usage has been made yet.");

            // wait 2 seconds to satisfy cooldown
            var usage = usageRepo.GetUsageForClient(clientToken);
            usage.LastRequestTime = usage.LastRequestTime.AddSeconds(-3);
            usageRepo.UpdateUsageForClient(clientToken, usage);

            // second request should pass cos limit = 3 (not used up), and cooldown (2secs) is satisfied
            bool secondRequest = composite.IsRequestAllowed(clientToken);
            Assert.IsTrue(secondRequest, "second request should pass if we satisfy both rules.");
        }
    }
}
