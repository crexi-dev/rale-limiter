using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RateLimiter.Constants;
using RateLimiter.Factories;
using RateLimiter.Models;

namespace RateLimiter.Tests.Rules
{
    public class TimeSpanSinceLastRequestRuleTest
    {
        [Test]
        public async Task IsRequestAllowedAsync_ReturnsTrue_When_LimitIsNotExceeded()
        {
            // Arrange
            var resourceId = "/api/path";
            var userId = "user1";
            var request = new RequestModel(resourceId, userId, string.Empty, string.Empty, string.Empty);
            var numRequestsAllowed = 1;
            var interval = new TimeSpan(hours: 0, minutes: 0, seconds: 5);
            var dataStoreFactory = new RateLimitDataStoreFactory();
            var ruleFactory = new RateLimitRuleFactory(dataStoreFactory);
            var rateLimitRule = ruleFactory.CreateRule(
                RateLimitRuleTypes.TimeSpanSinceLastRequest,
                RateLimitDataStoreTypes.ConcurrentInMemory,
                DataStoreKeyTypes.RequestsPerResource,
                numRequestsAllowed,
                interval);

            // Act
            var firstRequestAllowed = await rateLimitRule.IsRequestAllowedAsync(request);

            // Assert
            Assert.IsTrue(firstRequestAllowed);
        }

        [Test]
        public async Task IsRequestAllowedAsync_ReturnsFalse_When_LimitIsExceeded()
        {
            // Arrange
            var resourceId = "/api/path";
            int numRequestsAllowed = 1;
            var request = new RequestModel(resourceId, string.Empty, string.Empty, string.Empty, string.Empty);
            var interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 1);
            var dataStoreFactory = new RateLimitDataStoreFactory();
            var ruleFactory = new RateLimitRuleFactory(dataStoreFactory);
            var rateLimitRule = ruleFactory.CreateRule(
                RateLimitRuleTypes.TimeSpanSinceLastRequest,
                RateLimitDataStoreTypes.ConcurrentInMemory,
                DataStoreKeyTypes.RequestsPerResource,
                numRequestsAllowed,
                interval);

            // Act
            var firstRequestAllowed = await rateLimitRule.IsRequestAllowedAsync(request);
            var secondRequestAllowed = await rateLimitRule.IsRequestAllowedAsync(request);

            // Assert
            Assert.IsTrue(firstRequestAllowed, "Expected first request to be allowed");
            Assert.IsFalse(secondRequestAllowed, "Expected second request to be denied");
        }


        [Test]
        public async Task IsRequestAllowedAsync_MultipleRequests()
        {
            // Arrange
            var delayInMs = 11;
            var resourceId = "/api/path";
            var request = new RequestModel(resourceId, string.Empty, string.Empty, string.Empty, string.Empty);
            int numRequestsAllowed = 1;
            var interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 10);
            var dataStoreFactory = new RateLimitDataStoreFactory();
            var ruleFactory = new RateLimitRuleFactory(dataStoreFactory);
            var rateLimitRule = ruleFactory.CreateRule(
                RateLimitRuleTypes.TimeSpanSinceLastRequest,
                RateLimitDataStoreTypes.ConcurrentInMemory,
                DataStoreKeyTypes.RequestsPerResource,
                numRequestsAllowed,
                interval);

            // Act
            var firstRequestAllowed = await rateLimitRule.IsRequestAllowedAsync(request);
            var secondRequestAllowed = await rateLimitRule.IsRequestAllowedAsync(request);
            var thirdRequestAllowed = async () => await rateLimitRule.IsRequestAllowedAsync(request);
            var fourthRequestAllowed = async () => await rateLimitRule.IsRequestAllowedAsync(request);

            // Assert
            Assert.IsTrue(firstRequestAllowed, "Expected first request to be allowed");
            Assert.IsFalse(secondRequestAllowed, "Expected second request to be denied");
            Assert.That(() => thirdRequestAllowed(), Is.True.After(delayInMs), "Expected third request to be allowed");
            Assert.That(() => fourthRequestAllowed(), Is.False, "Expected fourth request to be denied");
        }
    }
}
