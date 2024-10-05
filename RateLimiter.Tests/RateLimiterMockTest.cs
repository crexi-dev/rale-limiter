using Moq;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Factory;
using RateLimiter.Model;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterMockTest
    {
        private Mock<IRateLimiterDataStore> _mockRateLimiterStore;
        private RateLimiterManager _rateLimiterManager;
        private IRateLimiterRuleFactory _rateLimiterRuleFactory;
        private RequestCount _requestCountRule;
        private TimeLimitBetweenRequests _timeLimitRule;

        [SetUp]
        public void Setup()
        {
            // Create a mock instance of the rate limit store
            _mockRateLimiterStore = new Mock<IRateLimiterDataStore>();

            // Set up the factory with the mocked store
            _rateLimiterRuleFactory = new RateLimiterRuleFactory(_mockRateLimiterStore.Object);

            // Set up the RateLimitManager with the factory
            _rateLimiterManager = new RateLimiterManager(_rateLimiterRuleFactory, _mockRateLimiterStore.Object);

            // Initialize Rules with mock store
            _requestCountRule = new RequestCount(100, TimeSpan.FromHours(1), _mockRateLimiterStore.Object);
            _timeLimitRule = new TimeLimitBetweenRequests(TimeSpan.FromSeconds(5), _mockRateLimiterStore.Object);
        }

        [Test]
        public void CheckRequest_AllowedRequest_ShouldReturnAllowed()
        {
            string clientId = "regular-client";
            string resource = "api/resource";

            _mockRateLimiterStore.Setup(s => s.GetClientData(clientId, resource))
                .Returns(new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow,
                    LastRequestTime = DateTime.UtcNow.AddSeconds(-10),
                    RequestCount = 0
                });

            var result = _rateLimiterManager.CheckRequest(clientId, resource);

            Assert.IsTrue(result.IsAllowed, "The request should be allowed.");
        }

        [Test]
        public void CheckRequest_ExceededRequestCount_ShouldReturnDenied()
        {
            string token = "basic-client";
            string resource = "api/resource";

            _mockRateLimiterStore.Setup(s => s.GetClientData(token, resource))
                .Returns(new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow,
                    LastRequestTime = DateTime.UtcNow,
                    RequestCount = 101 // the limit is 100 for basic clients
                });

            var result = _rateLimiterManager.CheckRequest(token, resource);

            Assert.IsFalse(result.IsAllowed, "The request should be denied as it exceeds the limit.");
        }

        [Test]
        public void RequestCountRule_ExceedsLimit_ShouldDenyRequest()
        {
            string token = "basic-client";
            string resource = "api/resource";

            _mockRateLimiterStore.Setup(s => s.GetClientData(token, resource))
                .Returns(new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow,
                    LastRequestTime = DateTime.UtcNow,
                    RequestCount = 101 // Exceeds limit
                });

            var result = _requestCountRule.CheckLimit(token, resource);

            Assert.IsFalse(result.IsAllowed, "The request should be denied as it exceeds the limit.");
        }

        [Test]
        public void RequestCountRule_WithinLimit_ShouldAllowRequest()
        {
            string clientId = "basic-client";
            string resource = "api/resource";

            _mockRateLimiterStore.Setup(s => s.GetClientData(clientId, resource))
                .Returns(new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow,
                    LastRequestTime = DateTime.UtcNow,
                    RequestCount = 99 // Below limit
                });

            var result = _requestCountRule.CheckLimit(clientId, resource);

            Assert.IsTrue(result.IsAllowed, "The request should be allowed since it is within the limit.");
        }

        [Test]
        public void TimeLimitRule_WithinLimit_ShouldAllowRequest()
        {
            string clientId = "basic-client";
            string resource = "api/resource";

            _mockRateLimiterStore.Setup(s => s.GetClientData(clientId, resource))
                .Returns(new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow.AddSeconds(-6),
                    LastRequestTime = DateTime.UtcNow.AddSeconds(-6),
                    RequestCount = 1
                });

            var result = _timeLimitRule.CheckLimit(clientId, resource);// 5 Second Limit

            Assert.IsTrue(result.IsAllowed, "The request should be allowed since it is within the limit.");
        }

        [Test]
        public void TimeLimitRule_ExceedsLimit_ShouldDenyRequest()
        {
            string token = "test-client";
            string resource = "api/resource1";

            _mockRateLimiterStore.Setup(s => s.GetClientData(token, resource))
                .Returns(new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow.AddSeconds(-2),
                    LastRequestTime = DateTime.UtcNow.AddSeconds(-3),
                    RequestCount = 1
                });

            var result = _timeLimitRule.CheckLimit(token, resource);// 5 Second apart

            Assert.IsFalse(result.IsAllowed, "The request should be denied as it exceeds the limit.");
        }        
    }
}
