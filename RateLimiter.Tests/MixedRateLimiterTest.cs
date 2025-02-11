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
    public class MixedRateLimiterTest
    {
        private readonly RuleOptions _ruleOptions;
        private IRateLimiter _subject;

        public MixedRateLimiterTest() 
        {
            var mockLogger = new Mock<ILogger<RateLimiter>>();

            _ruleOptions = new RuleOptions()
            {
                MaxCounts = 5,
                RuleType = Services.Enums.RuleType.Mixed,
                TimeStampFormat = "yyyyMMddHHmm",
                TimeSpan = TimeSpan.FromSeconds(30)
            };

            _subject = new RateLimiter(mockLogger.Object, _ruleOptions);
        }

        [Test]
        public async Task MixedRateLimiterTest_TestingTimeStamp_ShouldHandleIt()
        {
            // Arrange
            var resource = "www.myapi.com/customer";
            var token = "USA_" + Guid.NewGuid();
            var isRequestAllowed = true;
            var requestCount = 0;

            // Act
            for (; requestCount < 10 && isRequestAllowed; requestCount++)
            {
                //await _subject.IsRequestAllowed(token, resource);
                isRequestAllowed = await _subject.IsRequestAllowed(token, resource);
            }

            // Assert
            Assert.That(isRequestAllowed, Is.False);
            Assert.AreEqual(requestCount, _ruleOptions.MaxCounts + 1);
        }

        [Test]
        public async Task MixedRateLimiterTest_TestingTimeSpan_ShouldHandleIt()
        {
            // Arrange
            var dataContext = new InMemoryDataContext();
            var resource = "www.myapi.com/login";
            var token = "EU_" + Guid.NewGuid();

            for (var i = 0; i < 10; i++)
            {
                await dataContext.AddRequestLog(new RequestLog()
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
