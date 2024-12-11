using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using RateLimiter.Services;

namespace RateLimiter.Tests
{
    public class NorthAmericanRateLimiterPolicyTests
    {
        [Fact]
        public void TestRateLimitingNotAppliedBelowOrEqualToMaxRequests()
        {
            const int maxAllowedRequestsPerSlidingWindow = 3;
            const int windowLengthSeconds = 30;
            const int cacheExpirationMinutes = 5;
            var mockConfig = BuildTestConfig(maxAllowedRequestsPerSlidingWindow, windowLengthSeconds, cacheExpirationMinutes);

            string testApiKey = "test-api-key";
            var firstRequestTime = new DateTime(2024, 12, 9, 10, 10, 0);
            var secondRequestTime = firstRequestTime.AddSeconds(1);
            var thirdRequestTime = firstRequestTime.AddSeconds(2);

            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var clientBehaviorCache = new ClientBehaviorCache(memoryCache, mockConfig.Object);
            var policy = new NorthAmericanRateLimiterPolicy(clientBehaviorCache, mockConfig.Object);

            clientBehaviorCache.Add(testApiKey, firstRequestTime);
            clientBehaviorCache.Add(testApiKey, secondRequestTime);
            clientBehaviorCache.Add(testApiKey, thirdRequestTime);

            string msg = "No rate limiting expected if number of requests equal to max allowed";
            Assert.False(policy.IsApplicable(testApiKey, thirdRequestTime), msg);
        }

        [Fact]
        public void TestRateLimitingAppliedWhenMaxRequestsExceeded()
        {
            const int maxAllowedRequestsPerSlidingWindow = 3;
            const int windowLengthSeconds = 30;
            const int cacheExpirationMinutes = 5;
            var mockConfig = BuildTestConfig(maxAllowedRequestsPerSlidingWindow, windowLengthSeconds, cacheExpirationMinutes);

            string testApiKey = "test-api-key";
            var firstRequestTime = new DateTime(2024, 12, 9, 10, 10, 0);
            var secondRequestTime = firstRequestTime.AddSeconds(1);
            var thirdRequestTime = firstRequestTime.AddSeconds(28);
            var fourthRequestTime = firstRequestTime.AddSeconds(windowLengthSeconds);

            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var clientBehaviorCache = new ClientBehaviorCache(memoryCache, mockConfig.Object);
            var policy = new NorthAmericanRateLimiterPolicy(clientBehaviorCache, mockConfig.Object);

            clientBehaviorCache.Add(testApiKey, firstRequestTime);
            clientBehaviorCache.Add(testApiKey, secondRequestTime);
            clientBehaviorCache.Add(testApiKey, thirdRequestTime);
            clientBehaviorCache.Add(testApiKey, fourthRequestTime);

            string msg = "Rate limiting is expected when request count exceeds maximum allowed per period";
            Assert.True(policy.IsApplicable(testApiKey, thirdRequestTime), msg);
        }

        [Fact]
        public void TestRateLimiterDoesNotApplyWhenRequestsAreDistributedOverTime()
        {
            const int maxAllowedRequestsPerSlidingWindow = 3;
            const int windowLengthSeconds = 30;
            const int cacheExpirationMinutes = 5;
            var mockConfig = BuildTestConfig(maxAllowedRequestsPerSlidingWindow, windowLengthSeconds, cacheExpirationMinutes);

            string testApiKey = "test-api-key";
            var firstRequestTime = new DateTime(2024, 12, 9, 10, 10, 0);
            var secondRequestTime = firstRequestTime.AddSeconds(1);
            var thirdRequestTime = firstRequestTime.AddSeconds(2);

            // This request is more than 30 seconds removed from initial request. No rate limiting expected.
            var fourthRequestTime = firstRequestTime.AddSeconds(windowLengthSeconds + 1);

            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var clientBehaviorCache = new ClientBehaviorCache(memoryCache, mockConfig.Object);
            var policy = new NorthAmericanRateLimiterPolicy(clientBehaviorCache, mockConfig.Object);

            clientBehaviorCache.Add(testApiKey, firstRequestTime);
            clientBehaviorCache.Add(testApiKey, secondRequestTime);
            clientBehaviorCache.Add(testApiKey, thirdRequestTime);
            clientBehaviorCache.Add(testApiKey, fourthRequestTime);

            string msg = "Rate limiter should allow many requests as long as they don't fall into same sliding window";
            Assert.False(policy.IsApplicable(testApiKey, fourthRequestTime), msg);
        }

        private static Mock<IConfiguration> BuildTestConfig(int maxAllowedRequestsPerSlidingWindow, int windowLengthSeconds, int cacheExpirationMinutes)
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["RateLimiting:SlidingWindow:MaxRequests"]).Returns(maxAllowedRequestsPerSlidingWindow.ToString());
            mockConfig.Setup(c => c["RateLimiting:SlidingWindow:WindowLengthSeconds"]).Returns(windowLengthSeconds.ToString());
            mockConfig.Setup(c => c["RateLimiting:CacheExpirationMinutes"]).Returns(cacheExpirationMinutes.ToString());

            return mockConfig;
        }
    }

}
