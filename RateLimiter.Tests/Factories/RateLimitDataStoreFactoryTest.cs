using NUnit.Framework;
using RateLimiter.Constants;
using RateLimiter.Exceptions;
using RateLimiter.Factories;

namespace RateLimiter.Tests.Factories
{
    public class RateLimitDataStoreFactoryTest
    {
        [Test]
        public void CreateDataStore_Returns_DataStore_When_DataStoreTypeIsKnown()
        {
            // Arrange
            RateLimitDataStoreTypes dataStoreType = RateLimitDataStoreTypes.ConcurrentInMemory;
            var dataStoreFactory = new RateLimitDataStoreFactory();

            // Act
            var dataStore = dataStoreFactory.CreateDataStore(dataStoreType);

            // Assert
            Assert.NotNull(dataStore);
        }

        [Test]
        public void CreateDataStore_Throws_NotImplementedException_ForUnknownDataStoreType()
        {
            // Arrange
            RateLimitDataStoreTypes unimplementedDataStoreType = (RateLimitDataStoreTypes)10000;
            var dataStoreFactory = new RateLimitDataStoreFactory();

            // Act
            var datastoreDelegate = () => dataStoreFactory.CreateDataStore(unimplementedDataStoreType);

            // Assert
            Assert.Throws<DataStoreTypeNotImplementedException>(() => datastoreDelegate());
        }
    }
}
