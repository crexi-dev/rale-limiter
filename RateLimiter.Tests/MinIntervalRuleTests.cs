
using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RateLimiter.Tests
{
    public class MinIntervalRuleTests
    {
        private IDatabase _database;
        private MinIntervalRule _rule;
        private string _accessToken;

        [SetUp]
        public void Setup()
        {
            // Initialize Redis database
            _database = RedisHelper.Database;

            // Initialize the rule with a minimum interval of 60 seconds
            _rule = new MinIntervalRule(60, _database);

            // Example access token
            _accessToken = "test-token";

            // Clean up any existing data for the token
            Cleanup();
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up Redis
            _database.KeyDelete($"{_accessToken}:LastRequestTime");
        }

        /// <summary>
        /// First request: Should not rate limit (Result:False)
        /// </summary>
        [Test]
        public void ShouldRateLimit_FirstRequest()
        {
            // Arrange
            DateTime currentTime = DateTime.UtcNow;

            // Act
            bool result = _rule.ShouldRateLimit(_accessToken, currentTime);

            // Assert
            Assert.IsFalse(result);

            // Verify Redis value
            string lastRequestTime = _database.StringGet($"{_accessToken}:LastRequestTime");
            Assert.AreEqual(currentTime.ToString(), lastRequestTime);
        }

        /// <summary>
        /// Request before cooldown period : Should rate limit (Result: True)
        /// </summary>
        [Test]
        public void ShouldRateLimit_RequestTooSoon()
        {
            // Arrange
            DateTime lastRequestTime = DateTime.UtcNow;
            DateTime currentTime = lastRequestTime.AddSeconds(30); // Only 30 seconds later

            // Simulate a previous request
            _database.StringSet($"{_accessToken}:LastRequestTime", lastRequestTime.ToString());

            // Act
            bool result = _rule.ShouldRateLimit(_accessToken, currentTime);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Request after cooldown period : Should not rate limit (Result: False)
        /// </summary>
        [Test]
        public void ShouldRateLimit_RequestAfterMinInterval()
        {
            // Arrange
            DateTime lastRequestTime = DateTime.UtcNow.AddSeconds(-61); // More than 60 seconds ago
            DateTime currentTime = DateTime.UtcNow;

            // Simulate a previous request
            _database.StringSet($"{_accessToken}:LastRequestTime", lastRequestTime.ToString());

            // Act
            bool result = _rule.ShouldRateLimit(_accessToken, currentTime);

            // Assert
            Assert.IsFalse(result);

            // Verify Redis value
            string lastRequestTimeRedisValue = _database.StringGet($"{_accessToken}:LastRequestTime");
            Assert.AreEqual(currentTime.ToString(), lastRequestTimeRedisValue);
        }
    }
}
