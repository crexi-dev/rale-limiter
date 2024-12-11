using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using RateLimiter.Services;

namespace RateLimiter.Tests
{
    public class EuropeanRateLimiterPolicyTests
    {
        [Fact]
        public void TestRateLimitingNotAppliedBelowOrEqualToMaxRequests()
        {
            const int maxAllowedRequestsPerFixedWindow = 2;
            const string unitOfTime = "Minute";
            const int cacheExpirationMinutes = 5;
            var mockConfig = BuildTestConfig(maxAllowedRequestsPerFixedWindow, unitOfTime, cacheExpirationMinutes);

            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var clientBehaviorCache = new ClientBehaviorCache(memoryCache, mockConfig.Object);
            var policy = new EuropeanRateLimiterPolicy(clientBehaviorCache, mockConfig.Object);

            string testApiKey = "test-api-key";
            var initialRequestTime = new DateTime(2024, 12, 9, 10, 10, 0);
            string noRateLimitingExpectedMessage = $"Rate limiting should not apply until exceeding {maxAllowedRequestsPerFixedWindow} requests per period";

            clientBehaviorCache.Add(testApiKey, initialRequestTime);
            Assert.False(policy.IsApplicable(testApiKey, initialRequestTime), noRateLimitingExpectedMessage);

            var secondRequestTime = initialRequestTime.AddSeconds(58);
            clientBehaviorCache.Add(testApiKey, secondRequestTime);
            Assert.False(policy.IsApplicable(testApiKey, secondRequestTime), noRateLimitingExpectedMessage);
        }

        [Fact]
        public void TestRateLimitingAppliedWhenMaxRequestsExceeded()
        {
            const int maxAllowedRequestsPerFixedWindow = 2;
            const string unitOfTime = "Minute";
            const int cacheExpirationMinutes = 5;
            var mockConfig = BuildTestConfig(maxAllowedRequestsPerFixedWindow, unitOfTime, cacheExpirationMinutes);

            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var clientBehaviorCache = new ClientBehaviorCache(memoryCache, mockConfig.Object);
            var policy = new EuropeanRateLimiterPolicy(clientBehaviorCache, mockConfig.Object);

            string testApiKey = "test-api-key";
            var initialRequestTime = new DateTime(2024, 12, 9, 10, 10, 0);
            var secondRequestTime = initialRequestTime.AddSeconds(58);
            var thirdRequestTime = initialRequestTime.AddSeconds(59);

            clientBehaviorCache.Add(testApiKey, initialRequestTime);
            clientBehaviorCache.Add(testApiKey, secondRequestTime);
            clientBehaviorCache.Add(testApiKey, thirdRequestTime);

            string rateLimitingExpectedMessage = $"Rate limiting applies after exceeding {maxAllowedRequestsPerFixedWindow} requests per period";
            Assert.True(policy.IsApplicable(testApiKey, thirdRequestTime), rateLimitingExpectedMessage);
        }

        [Fact]
        public void TestCanExceedRateLimitByAllocatingRequestsAcrossDistinctFixedWindows()
        {
            const int maxAllowedRequestsPerFixedWindow = 2;
            const string unitOfTime = "Minute";
            const int cacheExpirationMinutes = 5;
            var mockConfig = BuildTestConfig(maxAllowedRequestsPerFixedWindow, unitOfTime, cacheExpirationMinutes);

            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var clientBehaviorCache = new ClientBehaviorCache(memoryCache, mockConfig.Object);
            var policy = new EuropeanRateLimiterPolicy(clientBehaviorCache, mockConfig.Object);

            string testApiKey = "test-api-key";
            var firstRequestTime = new DateTime(2024, 12, 9, 10, 10, 58);
            var secondRequestTime = new DateTime(2024, 12, 9, 10, 10, 59);
            var thirdRequestTime = new DateTime(2024, 12, 9, 10, 11, 1);
            var finalRequestTime = new DateTime(2024, 12, 9, 10, 11, 2);

            clientBehaviorCache.Add(testApiKey, firstRequestTime);
            clientBehaviorCache.Add(testApiKey, secondRequestTime);
            clientBehaviorCache.Add(testApiKey, thirdRequestTime);
            clientBehaviorCache.Add(testApiKey, finalRequestTime);

            string msg = "Should be able to exceed fixed window rate limit by allocating requests at end of window and at start of the next.";
            Assert.False(policy.IsApplicable(testApiKey, finalRequestTime), msg);
        }

        private static Mock<IConfiguration> BuildTestConfig(int maxAllowedRequestsPerFixedWindow, string unitOfTime, int cacheExpirationMinutes)
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["RateLimiting:FixedWindow:MaxRequests"]).Returns(maxAllowedRequestsPerFixedWindow.ToString());
            mockConfig.Setup(c => c["RateLimiting:FixedWindow:UnitOfTime"]).Returns(unitOfTime);
            mockConfig.Setup(c => c["RateLimiting:CacheExpirationMinutes"]).Returns(cacheExpirationMinutes.ToString());

            return mockConfig;
        }
    }
}
