using RateLimiter.Constants;
using RateLimiter.Stores;

namespace RateLimiter.Factories
{
    public interface IRateLimitDataStoreFactory
    {
        IRateLimitDataStore CreateDataStore(RateLimitDataStoreTypes dataStoreType);
    }
}
