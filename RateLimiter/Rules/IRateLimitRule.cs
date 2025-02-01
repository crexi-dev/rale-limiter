using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;

namespace RateLimiter.Rules
{
    public interface IRateLimitRule
    {
        bool IsRequestAllowed(ClientModel clientData, string resourceUrl, IMemoryCache memoryCache);
    }
}