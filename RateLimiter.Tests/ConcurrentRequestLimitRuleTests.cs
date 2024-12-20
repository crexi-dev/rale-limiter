using NUnit.Framework;
using RateLimiter.Rules;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class ConcurrentRequestLimitRuleTests
    {
        [Test]
        public void IsRequestAllowed_AllowsRequestsWithinLimit()
        {
            // Arrange
            var rule = new ConcurrentRequestLimitRule(2);
            var clientId = "client1";

            // Act
            var firstRequestAllowed = rule.IsRequestAllowed(clientId);
            var secondRequestAllowed = rule.IsRequestAllowed(clientId);

            // Assert
            Assert.IsTrue(firstRequestAllowed);
            Assert.IsTrue(secondRequestAllowed);
        }

        [Test]
        public void IsRequestAllowed_BlocksRequestsExceedingLimit()
        {
            // Arrange
            var rule = new ConcurrentRequestLimitRule(2);
            var clientId = "client1";

            // Act
            rule.IsRequestAllowed(clientId); // First request
            rule.IsRequestAllowed(clientId); // Second request
            var thirdRequestAllowed = rule.IsRequestAllowed(clientId); // Third request

            // Assert
            Assert.IsFalse(thirdRequestAllowed);
        }

        [Test]
        public void RequestCompleted_AllowsNewRequestAfterCompletion()
        {
            // Arrange
            var rule = new ConcurrentRequestLimitRule(2);
            var clientId = "client1";

            // Act
            rule.IsRequestAllowed(clientId); // First request
            rule.IsRequestAllowed(clientId); // Second request
            rule.RequestCompleted(clientId); // Complete one request
            var thirdRequestAllowed = rule.IsRequestAllowed(clientId); // Third request

            // Assert
            Assert.IsTrue(thirdRequestAllowed);
        }

        [Test]
        public void IsRequestAllowed_HandlesMultipleClients()
        {
            // Arrange
            var rule = new ConcurrentRequestLimitRule(2);
            var client1 = "client1";
            var client2 = "client2";

            // Act
            var client1FirstRequest = rule.IsRequestAllowed(client1);
            var client1SecondRequest = rule.IsRequestAllowed(client1);
            var client2FirstRequest = rule.IsRequestAllowed(client2);
            var client2SecondRequest = rule.IsRequestAllowed(client2);

            // Assert
            Assert.IsTrue(client1FirstRequest);
            Assert.IsTrue(client1SecondRequest);
            Assert.IsTrue(client2FirstRequest);
            Assert.IsTrue(client2SecondRequest);

            // Both clients should now be blocked
            Assert.IsFalse(rule.IsRequestAllowed(client1));
            Assert.IsFalse(rule.IsRequestAllowed(client2));
        }

        [Test]
        public void RequestCompleted_HandlesMultipleClients()
        {
            // Arrange
            var rule = new ConcurrentRequestLimitRule(2);
            var client1 = "client1";
            var client2 = "client2";

            // Act
            rule.IsRequestAllowed(client1); // First request for client1
            rule.IsRequestAllowed(client1); // Second request for client1
            rule.IsRequestAllowed(client2); // First request for client2
            rule.IsRequestAllowed(client2); // Second request for client2

            rule.RequestCompleted(client1); // Complete one request for client1
            rule.RequestCompleted(client2); // Complete one request for client2

            var client1ThirdRequest = rule.IsRequestAllowed(client1); // Third request for client1
            var client2ThirdRequest = rule.IsRequestAllowed(client2); // Third request for client2

            // Assert
            Assert.IsTrue(client1ThirdRequest);
            Assert.IsTrue(client2ThirdRequest);
        }
    }
}

