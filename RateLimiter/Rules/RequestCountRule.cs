using System;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;

namespace RateLimiter.Rules
{
    /// <summary>
    /// X requests per timespan
    /// </summary>
    public class RequestCountRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeSpan;

        public RequestCountRule(int maxRequests, TimeSpan timeSpan)
        {
            _maxRequests = maxRequests;
            _timeSpan = timeSpan;
        }

        public bool IsRequestAllowed(ClientModel clientData, string resourceName, IMemoryCache memoryCache)
        {
            string cacheKey = $"rateLimiter-count-{resourceName}-{clientData.ClientId}";

            if (!memoryCache.TryGetValue(cacheKey, out int requestCount))
            {
                memoryCache.Set(cacheKey, 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _timeSpan });

                return true;
            }

            if (requestCount >= _maxRequests)
            {
                return false;
            }

            memoryCache.Set(cacheKey, requestCount + 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _timeSpan });

            return true;
        }
    }
}