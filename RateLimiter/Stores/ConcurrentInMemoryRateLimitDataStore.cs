using RateLimiter.Models;
using System;
using System.Collections.Concurrent;

namespace RateLimiter.Stores
{
    public class ConcurrentInMemoryRateLimitDataStore : IRateLimitDataStore<RateLimitCounterModel>
    {
        private readonly ConcurrentDictionary<string, RateLimitCounterModel?> _store;

        public ConcurrentInMemoryRateLimitDataStore(ConcurrentDictionary<string, RateLimitCounterModel?> store)
        {
            _store = store;
        }

        public RateLimitCounterModel? Get(string key)
        {
            if (_store.TryGetValue(key, out var model))
            {
                return model;
            }

            return null;
        }

        public void Add(string key, RateLimitCounterModel value)
        {
            _store.GetOrAdd(key, value);
        }

        public void Update(string key, RateLimitCounterModel value)
        {
            _store.AddOrUpdate(key, value, (key, oldValue) => value);
        }
    }
}
