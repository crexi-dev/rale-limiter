using RateLimiter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RateLimiter.Persistence;

public class InMemoryCacheRepository(IProvideAccessToConfigurationData configurationRepository) : IProvideAccessToCachedData
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requestCache = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly Dictionary<string, List<RateLimitRuleConfiguration>> _configurationCache = [];
    private readonly Dictionary<string, int> _requestLifetimeCache = [];

    /// <summary>
    /// Inserts a timestamp (in UTC) into the list of request timestamps for the given key
    /// </summary>
    /// <param name="key">The key to use in the dictionary to save the timestamp</param>
    public void AddRequestByKey(string key)
    {
        List<DateTime> requests = _requestCache.GetOrAdd(key, []);
        requests.Add(DateTime.UtcNow);
        _requestCache.AddOrUpdate(key, requests, (key, value) => value);
    }

    /// <summary>
    /// Retrieves all request timestamps for the given
    /// </summary>
    /// <param name="key">The key to use in the dictionary to retrieve the timestamps</param>
    /// <returns>A list of timestamps for the given key</returns>
    public List<DateTime> GetRequestsByKey(string key)
    {
        return _requestCache.GetOrAdd(key, []);
    }

    /// <summary>
    /// Retrieves the configurations for a given key, either from a local cache instance or from persisted storage
    /// </summary>
    /// <remarks>If the value is retrieved from persisted storage, it is stored in the local cache for future use</remarks>
    /// <param name="key">The key to use to get configuration data</param>
    /// <returns>The list of configured rules for the given key</returns>
    public List<RateLimitRuleConfiguration> GetRuleConfigurationsByKey(string key)
    {
        if (!_configurationCache.TryGetValue(key, out List<RateLimitRuleConfiguration>? configurations))
        {
            configurations = configurationRepository.GetRuleConfigurationsByKey(key);
            _configurationCache[key] = configurations;
        }

        return configurations;
    }

    /// <summary>
    /// Locks the key to prevent cross-threading access to the list of request timestamps for the given key
    /// </summary>
    /// <param name="key">The key to lock</param>
    public void Lock(string key)
    {
        SemaphoreSlim locker = _locks.GetOrAdd(key, new SemaphoreSlim(1));
        locker.Wait();
    }

    /// <summary>
    /// Removes requests that are older than the configured number of seconds for the given key
    /// </summary>
    /// <remarks>If the value is retrieved from persisted storage, it is stored in the local cache for faster retrieval in the future</remarks>
    /// <param name="key">The key to use to determine the number of seconds to store requests</param>
    public void RemoveOldRequests(string key)
    {
        Lock(key);

        List<DateTime> requests = _requestCache.GetOrAdd(key, []);
        int numberOfSecondsToKeepRequestsBy = 0;
        if (!_requestLifetimeCache.TryGetValue(key, out numberOfSecondsToKeepRequestsBy))
        {
            numberOfSecondsToKeepRequestsBy = configurationRepository.GetNumberOfSecondsToKeepRequestsByKey(key);
            _requestLifetimeCache[key] = numberOfSecondsToKeepRequestsBy;
        }
        requests = requests.Where(r => r > DateTime.UtcNow.AddSeconds(-1 * numberOfSecondsToKeepRequestsBy)).ToList();
        _requestCache.AddOrUpdate(key, requests, (key, value) => value);

        Unlock(key);
    }

    /// <summary>
    /// Unlocks the key to allow the next waiting thread to be evaluated
    /// </summary>
    /// <param name="key">The key to unlock</param>
    public void Unlock(string key)
    {
        SemaphoreSlim locker = _locks.GetOrAdd(key, new SemaphoreSlim(1));
        locker.Release();
    }
}