using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.DataStore;
using RateLimiter.DataStore.Entities;
using RateLimiter.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class EuropeRateLimiterTest
    {
        private readonly RuleOptions _ruleOptions;
        private IRateLimiter _subject;

        public EuropeRateLimiterTest()
        {
            var mockLogger = new Mock<ILogger<RateLimiter>>();
            
            _ruleOptions = new RuleOptions()
            {
                MaxCounts = 5,
                TimeSpan = TimeSpan.FromSeconds(30),
                RuleType = Services.Enums.RuleType.Europe
            };

            _subject = new RateLimiter(mockLogger.Object, _ruleOptions);
        }

        [Test]
        public async Task EuropeRateLimiterTest_FirstRequest_ShouldAlwaysReturnTrue()
        {
            // Arrange
            var resource = "www.myapi.com/login";
            var token = "EU_" + Guid.NewGuid();

            // Act
            var isRequestAllowed = await _subject.IsRequestAllowed(token, resource);

            // Assert
            Assert.AreEqual(true, isRequestAllowed);
        }

        [Test]
        public async Task EuropeRateLimiterTest_MultipleRequestsInTimeSpan_ShouldReturnFalse()
        {
            // Arrange
            var rateLimitRepository = new RateLimitRepository(new InMemoryDataContext());
            var resource = "www.myapi.com/login";
            var token = "EU_" + Guid.NewGuid();

            for (var i = 0; i < 10; i++)
            {
                await rateLimitRepository.AddRequestLog(new RequestLog()
                {
                    ClientToken = token,
                    ResourceName = resource
                });
            }

            // Act
            var isRequestAllowed = await _subject.IsRequestAllowed(token, resource);

            // Assert
            Assert.AreEqual(false, isRequestAllowed);
        }
    }
}
