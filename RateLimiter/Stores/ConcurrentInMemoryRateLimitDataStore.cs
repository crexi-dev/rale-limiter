﻿using RateLimiter.Models;
using System;
using System.Collections.Concurrent;

namespace RateLimiter.Stores
{
    public class ConcurrentInMemoryRateLimitDataStore : IRateLimitDataStore
    {
        private readonly ConcurrentDictionary<string, RateLimitCounterModel?> _store;

        public ConcurrentInMemoryRateLimitDataStore()
        {
            _store = new ConcurrentDictionary<string, RateLimitCounterModel?>();
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
