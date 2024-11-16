using Crexi.Cache.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Crexi.Cache.Providers;

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

    public Task<List<string>> GetKeys(string keyPattern)
    {
        List<string> results = new();
        var fieldInfo = typeof(MemoryCache).GetField("_coherentState", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo == null)
        {
            return Task.FromResult(results);
        }
        var propertyInfo = fieldInfo.FieldType.GetProperty("EntriesCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        var value = fieldInfo.GetValue(_cache);
        var dict = propertyInfo?.GetValue(value) as dynamic;
        if (dict == null)
        {
            return Task.FromResult(results);
        }

        List<ICacheEntry> cacheCollectionValues = new List<ICacheEntry>();

        foreach (var cacheItem in dict)
        {
            ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);
            cacheCollectionValues.Add(cacheItemValue);
        }

        var regex = new Regex(GetKey(keyPattern), RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        List<string?> keys = cacheCollectionValues.Where(d => d != null &&  d.Key != null && d.Key.ToString() != null && regex.IsMatch(d.Key.ToString()!)).Select(d => d.Key.ToString()).ToList();
        foreach(string? k in keys)
        {
            if (k != null)
                results.Add(k);
        }
        return Task.FromResult(results);
    }

    public Task<Dictionary<string,T>> GetValues<T>(string keyPattern)
    {
        List<string> keys =  GetKeys(GetKey(keyPattern)).Result;
        var results = GetValues<T>(keys);
        return Task.FromResult(results.Result);
    }

    Task<Dictionary<string,T>> GetValues<T>(List<string> keys)
    {
        Dictionary <string,T> results = new Dictionary <string,T>();
        foreach (string k in keys)
        {
            var value = Get<T>(k);
            if (value != null && value.Result != null)
                results.TryAdd(k,value.Result);
        }
        return Task.FromResult(results);
    }
}
