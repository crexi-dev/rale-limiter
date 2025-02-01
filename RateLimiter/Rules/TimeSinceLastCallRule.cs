using System;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;

namespace RateLimiter.Rules
{
    /// <summary>
    /// A certain timespan has passed since the last call.
    /// </summary>
    public class TimeSinceLastCallRule : IRateLimitRule
    {
        private readonly TimeSpan _minInterval;

        public TimeSinceLastCallRule(TimeSpan minInterval)
        {
            _minInterval = minInterval;
        }

        public bool IsRequestAllowed(ClientModel clientData, string resourceName, IMemoryCache memoryCache)
        {
            string cacheKey = $"rateLimiter-lastCall-{resourceName}-{clientData.ClientId}";

            if (!memoryCache.TryGetValue(cacheKey, out DateTime lastRequestTime))
            {
                memoryCache.Set(cacheKey, DateTime.UtcNow);

                return true;
            }

            if (DateTime.UtcNow - lastRequestTime < _minInterval)
            {
                return false;
            }

            memoryCache.Set(cacheKey, DateTime.UtcNow);

            return true;
        }
    }
}