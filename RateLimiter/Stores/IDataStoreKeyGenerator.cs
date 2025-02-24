using RateLimiter.Constants;
using RateLimiter.Models;

namespace RateLimiter.Stores
{
    public interface IDataStoreKeyGenerator
    {
        string GenerateKey(RequestModel request);
    }
}
