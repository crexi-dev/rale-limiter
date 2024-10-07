using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Interfaces;
using System;

namespace RateLimiter.Storage
{
    public class RateLimiterLocalStorage : IRateLimiterStorage
    {
        public RateLimiterLocalStorage(
            IMemoryCache memoryCache
        ) {
            _memoryCache = memoryCache;
        }

        private readonly IMemoryCache _memoryCache;

        public IRateLimiterStorageEntry GetOrCreate(string key, TimeSpan expiry, IRateLimiterStorageEntry defaultValue)
        {
            return _memoryCache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = expiry;
                return defaultValue;
            }) ??
            throw new ArgumentNullException("Could not create an entry");
        }

        public IRateLimiterStorageEntry Set(string key, TimeSpan expiry, IRateLimiterStorageEntry value)
        {
            return _memoryCache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
        }

        public IRateLimiterStorageEntry Set(string key, IRateLimiterStorageEntry value)
        {
            return _memoryCache.Set(key, value);
        }
    }
}
