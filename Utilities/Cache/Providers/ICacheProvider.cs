﻿using Cache.Models;
namespace Cache.Providers;

public interface ICacheProvider
{
    Task Set<T>(string key, T value, CacheOptions? cacheOptions);
    Task<T?> Get<T>(string key);
    Task<List<string>> GetKeys(string keyPattern);
    Task<Dictionary<string, T>> GetDictionary<T>(string keyPattern);
    Task<List<T>> GetValues<T>(string keyPattern);
    Task Remove(string key);
    void Dispose();

}