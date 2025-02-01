using System;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;
using RateLimiter.Strategies;

namespace RateLimiter
{
    public class RateLimiterFactory : IRateLimiterFactory
    {
        private IMemoryCache _memoryCache;

        public RateLimiterFactory(IMemoryCache memoryCache) 
        {
            _memoryCache = memoryCache;
        }

        public IRateLimitService CreateRateLimiter(string resourceUrl, ClientModel clientData)
        {
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new ArgumentException($"{nameof(resourceUrl)} cannot be null or empty");
            }

            switch (resourceUrl)
            {
                case "resource1":
                    return new RateLimitResource1Service(_memoryCache, clientData);
                case "resource2":
                    return new RateLimitResource2Service(_memoryCache, clientData);
                case "resource3":
                    return new RateLimitResource3Service(_memoryCache, clientData);
                default:
                    throw new ArgumentException($"Cannot resolve resource mapping with url: {resourceUrl}");
            }
        }
    }
}