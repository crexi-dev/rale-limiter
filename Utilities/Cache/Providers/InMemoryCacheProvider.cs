using Cache.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Cache.Providers;

internal class InMemoryCacheProvider :  ICacheProvider, IDisposable
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheProvider(IMemoryCache memoryCache)
    {
        _cache = memoryCache;
    }
    public Task<T?> Get<T>(string key)
    {
        _cache.TryGetValue(GetKey(key), out T? result);
        return Task.FromResult(result);
    }

    public Task Remove(string key)
    {
       _cache.Remove(GetKey(key));
        return Task.CompletedTask;
    }

    public Task Set<T>(string key, T value, CacheOptions? cacheOptions)
    {
        var options = new MemoryCacheEntryOptions();

        if (cacheOptions != null)
        {
            if(cacheOptions.CacheExpiryOption == CacheExpiryOptionEnum.Sliding)
            {
                options.SetSlidingExpiration(TimeSpan.FromSeconds(cacheOptions.ExpiryTTLSeconds));
            }
            else
            {
                options.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheOptions.ExpiryTTLSeconds));
            }
        }
        _cache.Set(GetKey(key), value, options);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
    string GetKey(string key)
    {
        return key.Trim().ToLower();
    }

}
