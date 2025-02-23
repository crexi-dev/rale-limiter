using RateLimiter.Models;

namespace RateLimiter.Stores
{
    public interface IRateLimitDataStore
    {
        RateLimitCounterModel? Get(string key);
        void Add(string key, RateLimitCounterModel value);
        void Update(string key, RateLimitCounterModel value);
    }
}
