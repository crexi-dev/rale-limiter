using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Threading;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RequestsPerTimespanRuleTests
    {
        [Test]
        public void IsRequestAllowed_AllowsRequestsWithinLimit()
        {
            // Arrange
            var rule = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            var clientId = "client1";

            // Act & Assert
            for (int i = 1; i <= 4; i++)
            {
                Assert.IsTrue(rule.IsRequestAllowed(clientId));
            }
        }

        [Test]
        public void IsRequestAllowed_BlocksRequestsExceedingLimit()
        {
            // Arrange
            var rule = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            var clientId = "client1";

            // Act
            for (int i = 1; i <= 5; i++)
            {
                rule.IsRequestAllowed(clientId);
            }

            // Assert
            Assert.IsFalse(rule.IsRequestAllowed(clientId)); // 6th request should be blocked
        }

        [Test]
        public void IsRequestAllowed_AllowsRequestsAfterTimespan()
        {
            // Arrange
            var rule = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(1));
            var clientId = "client1";

            // Act
            rule.IsRequestAllowed(clientId);

            // Wait for the timespan to elapse
            Thread.Sleep(1100);

            // Assert
            Assert.IsTrue(rule.IsRequestAllowed(clientId));
        }

        [Test]
        public void IsRequestAllowed_HandlesMultipleClients()
        {
            // Arrange
            var rule = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            var client1 = "client1";
            var client2 = "client2";

            // Act & Assert
            for (int i = 1; i <= 5; i++)
            {
                Assert.IsTrue(rule.IsRequestAllowed(client1));
                Assert.IsTrue(rule.IsRequestAllowed(client2));
            }

            // Both clients should now be blocked
            Assert.IsFalse(rule.IsRequestAllowed(client1));
            Assert.IsFalse(rule.IsRequestAllowed(client2));
        }
    }
}
