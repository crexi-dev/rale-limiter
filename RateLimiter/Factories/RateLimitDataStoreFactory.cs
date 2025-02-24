using RateLimiter.Constants;
using RateLimiter.Exceptions;
using RateLimiter.Stores;

namespace RateLimiter.Factories
{
    public class RateLimitDataStoreFactory : IRateLimitDataStoreFactory
    {
        public IRateLimitDataStore CreateDataStore(RateLimitDataStoreTypes dataStoreType)
        {
            switch (dataStoreType)
            {
                case RateLimitDataStoreTypes.ConcurrentInMemory:
                    return new ConcurrentInMemoryRateLimitDataStore();
                default:
                    throw new DataStoreTypeNotImplementedException();
            }
        }
    }
}
