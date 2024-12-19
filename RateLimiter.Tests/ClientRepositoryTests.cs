using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Repositories;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class ClientRepositoryTests
    {
        private IClientRepository _clientRepository;

        [SetUp]
        public void Setup()
        {
            _clientRepository = new ClientRepository();
        }

        [Test]
        public void GetClientRegionByAccessToken_ReturnsCorrectRegion_ForKnownClient()
        {
            // Arrange
            var client1Token = "client1";
            var client2Token = "client2";
            var client3Token = "client3";
            var client4Token = "client4";

            // Act
            var client1Region = _clientRepository.GetClientRegionByAccessToken(client1Token);
            var client2Region = _clientRepository.GetClientRegionByAccessToken(client2Token);
            var client3Region = _clientRepository.GetClientRegionByAccessToken(client3Token);
            var client4Region = _clientRepository.GetClientRegionByAccessToken(client4Token);

            // Assert
            Assert.AreEqual("US", client1Region);
            Assert.AreEqual("EU", client2Region);
            Assert.AreEqual("US", client3Region);
            Assert.AreEqual("EU", client4Region);
        }

        [Test]
        public void GetClientRegionByAccessToken_ReturnsUnknown_ForUnknownClient()
        {
            // Arrange
            var unknownClientToken = "unknownClient";

            // Act
            var region = _clientRepository.GetClientRegionByAccessToken(unknownClientToken);

            // Assert
            Assert.AreEqual("Unknown", region);
        }
    }
}
