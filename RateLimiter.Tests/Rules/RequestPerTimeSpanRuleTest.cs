using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;
using RateLimiter.Stores;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules
{
    internal class RequestPerTimeSpanRuleTest
    {
        [Test]
        public async Task RequestPerTimeSpanRule_IsWithinLimit_ReturnsTrue_When_LimitIsNotExceeded()
        {
            // Arrange
            var resourceId = "/api/path";
            var userId = "user1";
            var request = new RequestModel(resourceId, userId, string.Empty, string.Empty, string.Empty);
            var numRequestsAllowed = 1;
            var interval = new TimeSpan(hours: 0, minutes: 0, seconds: 5);
            var loggerMock = new Mock<ILogger<RequestsPerTimeSpanRule>>();
            var dataStoreMock = new Mock<IRateLimitDataStore<RateLimitCounterModel>>();
            var rateLimitRule = new RequestsPerTimeSpanRule(numRequestsAllowed, interval, dataStoreMock.Object, loggerMock.Object);

            // Act
            var firstRequestAllowed = await rateLimitRule.IsWithinLimitAsync(request);

            // Assert
            Assert.IsTrue(firstRequestAllowed);
        }

        [Test]
        public async Task RequestPerTimeSpanRule_IsWithinLimit_ReturnsFalse_When_LimitIsExceeded()
        {
            // Arrange
            var resourceId = "/api/path";
            int numRequestsAllowed = 1;
            var request = new RequestModel(resourceId, string.Empty, string.Empty, string.Empty, string.Empty);
            var interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 1);
            var loggerMock = new Mock<ILogger<RequestsPerTimeSpanRule>>();
            var concurrentDict = new ConcurrentDictionary<string, RateLimitCounterModel>();
            var dataStore = new ConcurrentInMemoryRateLimitDataStore(concurrentDict);
            var rateLimitRule = new RequestsPerTimeSpanRule(numRequestsAllowed, interval, dataStore, loggerMock.Object);

            // Act
            var firstRequestAllowed = await rateLimitRule.IsWithinLimitAsync(request);
            var secondRequestAllowed = await rateLimitRule.IsWithinLimitAsync(request);

            // Assert
            Assert.IsTrue(firstRequestAllowed, "Expected first request to be allowed");
            Assert.IsFalse(secondRequestAllowed, "Expected second request to be denied");
        }


        [Test]
        public async Task RequestPerTimeSpanRule_IsWithinLimit_MultipleRequests()
        {
            // Arrange
            var delayInMs = 1001;
            var resourceId = "/api/path";
            var request = new RequestModel(resourceId, string.Empty, string.Empty, string.Empty, string.Empty);
            int numRequestsAllowed = 1;
            var interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 1);
            var loggerMock = new Mock<ILogger<RequestsPerTimeSpanRule>>();
            var concurrentDict = new ConcurrentDictionary<string, RateLimitCounterModel>();
            var dataStore = new ConcurrentInMemoryRateLimitDataStore(concurrentDict);
            var rateLimitRule = new RequestsPerTimeSpanRule(numRequestsAllowed, interval, dataStore, loggerMock.Object);

            // Act
            var firstRequestAllowed = await rateLimitRule.IsWithinLimitAsync(request);
            var secondRequestAllowed = await rateLimitRule.IsWithinLimitAsync(request);
            var thirdRequestAllowed = async () => await rateLimitRule.IsWithinLimitAsync(request);

            // Assert
            Assert.IsTrue(firstRequestAllowed, "Expected first request to be allowed");
            Assert.IsFalse(secondRequestAllowed, "Expected second request to be denied");
            Assert.That(() => thirdRequestAllowed(), Is.True.After(delayInMs), "Expected third request to be allowed");
            Assert.That(async () => await rateLimitRule.IsWithinLimitAsync(request), Is.False, "Expected fourth request to be denied");
        }
    }
}
