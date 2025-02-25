using NUnit.Framework;
using RateLimiter.Constants;
using RateLimiter.Exceptions;
using RateLimiter.Models;
using RateLimiter.Stores;

namespace RateLimiter.Tests.Stores
{
    public class DataStoreKeyGeneratorTest
    {
        [Test]
        public void GenerateKey_Returns_ExpectedKey_When_KeyTypeIsKnown()
        {
            // Arrange
            var ipAddress = "67.88.121.44";
            var organizationId = "Simple Software Solutions Inc (SSSI)";
            var userId = "100";
            var region = "us-west";
            var requestPath = "api/profiles";
            var request = new RequestModel(requestPath, userId, organizationId, ipAddress, region);
            DataStoreKeyTypes dataStoreKeyType = DataStoreKeyTypes.RequestsPerOrganizationUserPerResource;
            var dataStoreKeyGenerator = new DataStoreKeyGenerator(dataStoreKeyType);
            var expectedDataStoreKey = $"{request.UserId}:{request.OrganizationId}:{request.RequestPath}";

            // Act
            var actualDataStoreKey = dataStoreKeyGenerator.GenerateKey(request);

            // Assert
            Assert.AreEqual(expectedDataStoreKey, actualDataStoreKey);
        }

        [Test]
        public void CreateDataStore_Throws_NotImplementedException_ForUnknownDataStoreType()
        {
            // Arrange
            // Arrange
            var ipAddress = "67.88.121.44";
            var organizationId = "Simple Software Solutions Inc (SSSI)";
            var userId = "100";
            var region = "us-west";
            var requestPath = "api/profiles";
            var request = new RequestModel(requestPath, userId, organizationId, ipAddress, region);
            var expectedDataStoreKey = $"{request.UserId}:{request.OrganizationId}:{request.RequestPath}";

            DataStoreKeyTypes unknownDataStoreKeyType = (DataStoreKeyTypes)10000;
            var dataStoreKeyGenerator = new DataStoreKeyGenerator(unknownDataStoreKeyType);

            // Act
            var dataStoreKeyGeneratorDelegate = () => dataStoreKeyGenerator.GenerateKey(request);

            // Assert
            Assert.Throws<DataStoreKeyTypeNotImplementedException>(() => dataStoreKeyGeneratorDelegate());
        }
    }
}
