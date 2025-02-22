using RateLimiter.Models;

namespace RateLimiter.Stores
{
    public interface IRateLimitDataStore<T> where T : class
    {
        T? Get(string key);
        void Add(string key, RateLimitCounterModel value);
        void Update(string key, T value);
    }
}
