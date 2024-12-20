using Moq;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RegionBasedRuleTests
    {
        private Mock<IClientRepository> _clientRepositoryMock;
        private RequestsPerTimespanRule _usRule;
        private TimespanSinceLastCallRule _euRule;
        private RegionBasedRule _regionBasedRule;

        [SetUp]
        public void Setup()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _usRule = new RequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            _euRule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
            _regionBasedRule = new RegionBasedRule(_usRule, _euRule, _clientRepositoryMock.Object);
        }

        [Test]
        public void IsRequestAllowed_UsesUsRuleForUsRegion()
        {
            // Arrange
            var clientId = "client1";
            _clientRepositoryMock.Setup(repo => repo.GetClientRegionByAccessToken(clientId)).Returns("US");

            // Act
            var result = _regionBasedRule.IsRequestAllowed(clientId);

            // Assert
            Assert.IsTrue(result);
            _clientRepositoryMock.Verify(repo => repo.GetClientRegionByAccessToken(clientId), Times.Once);
        }

        [Test]
        public void IsRequestAllowed_UsesEuRuleForEuRegion()
        {
            // Arrange
            var clientId = "client2";
            _clientRepositoryMock.Setup(repo => repo.GetClientRegionByAccessToken(clientId)).Returns("EU");

            // Act
            var result = _regionBasedRule.IsRequestAllowed(clientId);

            // Assert
            Assert.IsTrue(result);
            _clientRepositoryMock.Verify(repo => repo.GetClientRegionByAccessToken(clientId), Times.Once);
        }

        [Test]
        public void IsRequestAllowed_ReturnsFalseForUnknownRegion()
        {
            // Arrange
            var clientId = "client3";
            _clientRepositoryMock.Setup(repo => repo.GetClientRegionByAccessToken(clientId)).Returns("Unknown");

            // Act
            var result = _regionBasedRule.IsRequestAllowed(clientId);

            // Assert
            Assert.IsFalse(result);
            _clientRepositoryMock.Verify(repo => repo.GetClientRegionByAccessToken(clientId), Times.Once);
        }

        [Test]
        public void IsRequestAllowed_HandlesMultipleClients()
        {
            // Arrange
            var client1 = "client1";
            var client2 = "client2";
            _clientRepositoryMock.Setup(repo => repo.GetClientRegionByAccessToken(client1)).Returns("US");
            _clientRepositoryMock.Setup(repo => repo.GetClientRegionByAccessToken(client2)).Returns("EU");

            // Act & Assert
            // Client 1
            for (int i = 1; i <= 5; i++)
            {
                Assert.IsTrue(_regionBasedRule.IsRequestAllowed(client1));
            }
            Assert.IsFalse(_regionBasedRule.IsRequestAllowed(client1));

            // Client 2
            Assert.IsTrue(_regionBasedRule.IsRequestAllowed(client2));
            Assert.IsFalse(_regionBasedRule.IsRequestAllowed(client2)); // Subsequent immediate request should be blocked

        }
    }
}

