using RateLimiter.Models;
using RateLimiter.Stores;
using System;

namespace RateLimiter.Factories
{
    public class RateLimitDataStoreFactory : IRateLimitDataStoreFactory
    {
        private static readonly string UnknownDataStoreError = "Unknown RateLimitDataStoreType: {0}";

        public IRateLimitDataStore CreateDataStore(RateLimitDataStoreTypes dataStoreType)
        {
            switch (dataStoreType)
            {
                case RateLimitDataStoreTypes.ConcurrentInMemory:
                    return new ConcurrentInMemoryRateLimitDataStore();
                default:
                    var errorMessage = string.Format(UnknownDataStoreError, dataStoreType.ToString());
                    throw new NotImplementedException(errorMessage);
            }
        }
    }
}
