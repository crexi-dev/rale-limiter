using NUnit.Framework;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class TimespanSinceLastCallRuleTest
    {
        [Test]
        public void Should_AllowRequest_IfEnoughTimePassed()
        {
            var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            var clientId = "client2";

            Assert.IsTrue(rule.IsRequestAllowed(clientId));

            // Wait for time to pass
            System.Threading.Thread.Sleep(1100);
            Assert.IsTrue(rule.IsRequestAllowed(clientId));
        }

        [Test]
        public void Should_DenyRequest_IfNotEnoughTimePassed()
        {
            var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(2));
            var clientId = "client2";

            Assert.IsTrue(rule.IsRequestAllowed(clientId));

            // This request should be denied since the timespan is less than 2 seconds
            Assert.IsFalse(rule.IsRequestAllowed(clientId));
        }
    }

}
