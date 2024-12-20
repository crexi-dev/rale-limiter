using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Threading;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class TimespanSinceLastCallRuleTests
    {
        [Test]
        public void IsRequestAllowed_AllowsFirstRequest()
        {
            // Arrange
            var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            var clientId = "client1";

            // Act
            var result = rule.IsRequestAllowed(clientId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsRequestAllowed_BlocksRequestWithinTimespan()
        {
            // Arrange
            var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            var clientId = "client1";

            // Act
            rule.IsRequestAllowed(clientId);

            // Assert
            Assert.IsFalse(rule.IsRequestAllowed(clientId)); // 2nd request should be blocked
        }

        [Test]
        public void IsRequestAllowed_AllowsRequestAfterTimespan()
        {
            // Arrange
            var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            var clientId = "client1";

            // Act
            rule.IsRequestAllowed(clientId);
            Thread.Sleep(1100);

            // Assert
            Assert.IsTrue(rule.IsRequestAllowed(clientId));
        }

        [Test]
        public void IsRequestAllowed_HandlesMultipleClients()
        {
            // Arrange
            var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            var client1 = "client1";
            var client2 = "client2";

            // Act
            var result1 = rule.IsRequestAllowed(client1);
            var result2 = rule.IsRequestAllowed(client2);

            // Assert
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);

            // Act
            result1 = rule.IsRequestAllowed(client1);
            result2 = rule.IsRequestAllowed(client2);

            // Assert
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }
    }
}
