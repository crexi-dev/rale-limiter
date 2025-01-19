using Moq;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Providers;
using RateLimiter.Rules;
using RateLimiter.Store;
using System;

namespace RateLimiter.Tests.Rules
{
    public class MinimumTimeIntervalRuleTests
    {
        private Mock<IDataStore> _mockDataStore;
        private Mock<IDateTimeProvider> _mockDataTimeProvider;
        private MinimumTimeIntervalRule _rule;

        private const string ClientToken = "client1";
        private const string Uri = "http://example.com";
        private const int MinTimeIntervalInSeconds = 10;

        [SetUp]
        public void Setup()
        {
            _mockDataStore = new Mock<IDataStore>();
            _mockDataTimeProvider = new Mock<IDateTimeProvider>();
            _rule = new MinimumTimeIntervalRule(_mockDataStore.Object, _mockDataTimeProvider.Object, MinTimeIntervalInSeconds);
        }

        [Test]
        public void IsAllowed_ShouldReturnFalse_WhenTimeIntervalIsLessThanOrEqualToMinimum()
        {
            var lastRequestTimestamp = DateTime.UtcNow.AddSeconds(-MinTimeIntervalInSeconds);
            var lastRequest = new ClientRequestModel("test", lastRequestTimestamp);
            _mockDataStore.Setup(ds => ds.GetLastRequestByClient(ClientToken)).Returns(lastRequest);
            _mockDataTimeProvider.Setup(t => t.UtcNow).Returns(lastRequestTimestamp.AddSeconds(MinTimeIntervalInSeconds-1));
            var result = _rule.IsAllowed(ClientToken, Uri);

            Assert.IsFalse(result);
            _mockDataStore.Verify(ds => ds.AddClientRequest(ClientToken, Uri), Times.Once);
        }

        [Test]
        public void IsAllowed_ShouldReturnTrue_WhenNoPreviousRequestExists()
        {
            _mockDataStore.Setup(ds => ds.GetLastRequestByClient(ClientToken)).Returns((ClientRequestModel)null);

            var result = _rule.IsAllowed(ClientToken, Uri);

            Assert.IsTrue(result);
            _mockDataStore.Verify(ds => ds.AddClientRequest(ClientToken, Uri), Times.Once);
        }

        [Test]
        public void IsAllowed_ShouldCallAddClientRequest_WhenCheckingAllowance()
        {
            var lastRequestTimestamp = DateTime.UtcNow.AddSeconds(-MinTimeIntervalInSeconds - 1);
            var lastRequest = new ClientRequestModel("", lastRequestTimestamp);
            _mockDataStore.Setup(ds => ds.GetLastRequestByClient(ClientToken)).Returns(lastRequest);

            _rule.IsAllowed(ClientToken, Uri);

            _mockDataStore.Verify(ds => ds.AddClientRequest(ClientToken, Uri), Times.Once);
        }
    }
}
