using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace RateLimiter.Services;

[ExcludeFromCodeCoverageAttribute]
public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T Get<T>(string key)
    {
        return _cache.Get<T>(key);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public T Set<T>(string key, T value, MemoryCacheEntryOptions cacheOptions)
    {
        _cache.Set<T>(key, value, cacheOptions);
        return value;
    }

    public T Set<T>(string key, T value)
    {
        _cache.Set<T>(key, value);
        return value;
    }
}
