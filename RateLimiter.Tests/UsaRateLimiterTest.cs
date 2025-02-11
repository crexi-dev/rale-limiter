using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class UsaRateLimiterTest
    {
        private readonly RuleOptions _ruleOptions;
        private IRateLimiter _subject;

        public UsaRateLimiterTest()
        {
            var mockLogger = new Mock<ILogger<RateLimiter>>();

            _ruleOptions = new RuleOptions()
            {
                MaxCounts = 5,
                RuleType = Services.Enums.RuleType.USA,
                TimeStampFormat = "yyyyMMddHHmm"
            };

            _subject = new RateLimiter(mockLogger.Object, _ruleOptions);
        }

        [Test]
        public async Task UsaRateLimiterTest_FirstRequest_ShouldAlwaysReturnTrue()
        {
            // Arrange
            var resource = "www.myapi.com/user";
            var token = "USA_" + Guid.NewGuid();

            // Act
            var isRequestAllowed = await _subject.IsRequestAllowed(token, resource);

            // Assert
            Assert.That(isRequestAllowed, Is.True);
        }

        [Test]
        public async Task UsaRateLimiterTest_WhenMaxCountsExcessed_ShouldReturnFalse()
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
            Assert.AreEqual(requestCount, _ruleOptions.MaxCounts+1);
        }
    }
}
