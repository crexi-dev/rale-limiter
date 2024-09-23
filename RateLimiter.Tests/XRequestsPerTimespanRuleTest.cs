using NUnit.Framework;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class XRequestsPerTimespanRuleTest
    {
        [Test]
        public void Should_AllowRequests_WithinLimit()
        {
            var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1));
            var clientId = "client1";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rule.IsRequestAllowed(clientId));
            }
        }

        [Test]
        public void Should_DenyRequests_ExceedingLimit()
        {
            var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1));
            var clientId = "client1";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rule.IsRequestAllowed(clientId));
            }

            // Sixth request should be denied
            Assert.IsFalse(rule.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_AllowRequests_AfterTimeWindow()
        {
            var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(1));
            var clientId = "client1";

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rule.IsRequestAllowed(clientId));
            }

            // Wait for window to reset
            System.Threading.Thread.Sleep(1100);

            Assert.IsTrue(rule.IsRequestAllowed(clientId));
        }
    }

}
