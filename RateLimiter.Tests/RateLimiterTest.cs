using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Threading;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterTests
    {
        private RateLimiter _rateLimiter;

        [SetUp]
        public void Setup()
        {
            _rateLimiter = new RateLimiter();
        }

        [Test]
        public void IsRequestAllowed_AllowsRequestWhenNoRules()
        {
            // Arrange
            var resource = "resource1";
            var clientId = "client1";

            // Act
            var result = _rateLimiter.IsRequestAllowed(resource, clientId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsRequestAllowed_AllowsRequestWhenSingleRuleAllows()
        {
            // Arrange
            var resource = "resource1";
            var clientId = "client1";
            var rule = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            _rateLimiter.AddRule(resource, rule);

            // Act
            var result = _rateLimiter.IsRequestAllowed(resource, clientId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsRequestAllowed_BlocksRequestWhenSingleRuleBlocks()
        {
            // Arrange
            var resource = "resource1";
            var clientId = "client1";
            var rule = new RequestsPerTimespanRule(1, TimeSpan.FromSeconds(10));
            _rateLimiter.AddRule(resource, rule);

            // Act
            _rateLimiter.IsRequestAllowed(resource, clientId); // First request
            var result = _rateLimiter.IsRequestAllowed(resource, clientId); // Second request

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsRequestAllowed_AllowsRequestWhenAllCombinedRulesAllow()
        {
            // Arrange
            var resource = "resource1";
            var clientId = "client1";
            var rule1 = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            var rule2 = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            _rateLimiter.AddRule(resource, rule1);
            _rateLimiter.AddRule(resource, rule2);

            // Act
            var result = _rateLimiter.IsRequestAllowed(resource, clientId);
            Thread.Sleep(1100); // Wait for the timespan to elapse

            // Assert
            Assert.IsTrue(_rateLimiter.IsRequestAllowed(resource, clientId));
        }

        [Test]
        public void IsRequestAllowed_BlocksRequestWhenAnyCombinedRuleBlocks()
        {
            // Arrange
            var resource = "resource1";
            var clientId = "client1";
            var rule1 = new RequestsPerTimespanRule(1, TimeSpan.FromSeconds(10));
            var rule2 = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            _rateLimiter.AddRule(resource, rule1);
            _rateLimiter.AddRule(resource, rule2);

            // Act
            _rateLimiter.IsRequestAllowed(resource, clientId); // First request
            var result = _rateLimiter.IsRequestAllowed(resource, clientId); // Second request

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsRequestAllowed_HandlesMultipleResources()
        {
            // Arrange
            var resource1 = "resource1";
            var resource2 = "resource2";
            var clientId = "client1";
            var rule1 = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            var rule2 = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            _rateLimiter.AddRule(resource1, rule1);
            _rateLimiter.AddRule(resource2, rule2);

            // Act
            var result1 = _rateLimiter.IsRequestAllowed(resource1, clientId);
            var result2 = _rateLimiter.IsRequestAllowed(resource2, clientId);

            // Assert
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
        }
    }
}
