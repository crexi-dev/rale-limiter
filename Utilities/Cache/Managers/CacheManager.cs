using Cache.Providers;
using Cache.Models;

namespace Utilities.Cache.Managers;

public class CacheManager : ICacheManager, IDisposable
{
    private readonly ICacheProvider _cacheProvider;

    public CacheManager(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task<T?> Get<T>(string key)
    {
        return await _cacheProvider.Get<T>(key);
    }

    public async Task Remove(string key)
    {
        await _cacheProvider.Remove(key);
    }

    public async Task Set<T>(string key, T value, CacheOptions cacheOptions)
    {
        await _cacheProvider.Set<T>(key, value, cacheOptions);
    }

    public void Dispose()
    {
        _cacheProvider.Dispose();
    }

    public async Task<List<string>> GetKeys(string keyPattern)
    {
        return await _cacheProvider.GetKeys(keyPattern);
    }

    public async Task<Dictionary<string, T>> GetValues<T>(string keyPattern)
    {
        return await _cacheProvider.GetDictionary<T>(keyPattern);
    }
}

