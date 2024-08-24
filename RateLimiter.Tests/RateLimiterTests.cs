using NUnit.Framework;
using StackExchange.Redis;
using RateLimiter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RateLimiter.Tests
{
    /*
     Note:  These tests are specifically designed for the RateLimiter class and the current rules defined in the RateLimitRules.json file.
            They verify whether the correct rules are retrieved from the JSON file based on the ruleType and region. 
            Ideally, we would log names of the rules applied to each request or store the executed rule names in cache.
            This approach serves as a temporary workaround for testing purposes."
     */
    public class RateLimiterTests
    {
        private IDatabase _database;
        private RateLimiter _rateLimiter;
        private string _accessToken;

        [SetUp]
        public void Setup()
        {
            // Initialize Redis database
            _database = RedisHelper.Database;

            // Example access token
            _accessToken = "test-token";

            // Clean up any existing data for the token
            Cleanup();
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up Redis keys related to the token
            _database.KeyDelete($"{_accessToken}:RequestCount");
            _database.KeyDelete($"{_accessToken}:LastRequestTime");
        }

        /// <summary>
        /// Checks if Ratelimiter runs Global rules
        /// </summary>
        [Test]
        public void IsRequestRateLimited_GlobalRules()
        {
            // Arrange
            string pattern = $"{_accessToken}:*";
            _rateLimiter = new RateLimiter(_database);

            // Act
            bool result = _rateLimiter.IsRequestRateLimited(_accessToken, RuleType.Global, null);

            // Assert
            Assert.IsFalse(result);
            IEnumerable<RedisKey> keys = RedisHelper.GetKeysByPattern(_database, pattern);
            
            //Since the global rule has a ruleStrategy of FixedNumOfRequests, it will include two keys.
            Assert.AreEqual(2,keys.Count());
        }

        /// <summary>
        /// Checks if Ratelimiter runs EU rule
        /// </summary>
        [Test]
        public void IsRequestRateLimited_RegionalRules_EU()
        {
            // Arrange
            string pattern = $"{_accessToken}:*";
            _rateLimiter = new RateLimiter(_database);

            // Act
            bool result = _rateLimiter.IsRequestRateLimited(_accessToken, RuleType.Regional, "EU");

            // Assert
            Assert.IsFalse(result);
            IEnumerable<RedisKey> keys = RedisHelper.GetKeysByPattern(_database, pattern);

            //Since the global rule has a ruleStrategy of MinInterval, it will include 1 key and RequestCount will be null as it's not applicable to MinInterval
            Assert.AreEqual(1, keys.Count());
            Assert.AreEqual(RedisValue.Null, _database.StringGet($"{_accessToken}:RequestCount"));
        }

        /// <summary>
        /// Checks if Ratelimiter runs US rule
        /// </summary>
        [Test]
        public void IsRequestRateLimited_RegionalRules_US()
        {
            // Arrange
            string pattern = $"{_accessToken}:*";
            _rateLimiter = new RateLimiter(_database);

            // Act
            bool result = _rateLimiter.IsRequestRateLimited(_accessToken, RuleType.Regional, "US");

            // Assert
            Assert.IsFalse(result);
            IEnumerable<RedisKey> keys = RedisHelper.GetKeysByPattern(_database, pattern);

            //Since the global rule has a ruleStrategy of FixedNumOfRequests, it will include 2 keys.
            Assert.AreEqual(2, keys.Count());
        }
    }
}
