using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Enums;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class RequestsPerTimeSpanRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeSpan;
        private readonly MemoryCache _cache;

        public RequestsPerTimeSpanRule(int maxRequests, TimeSpan timeSpan)
        {
            _maxRequests = maxRequests;
            _timeSpan = timeSpan;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<bool> IsRequestAllowedAsync(string accessToken, DateTime requestTime, Region region = Region.ALL_REGIONS)
        {
            var timestamps = _cache.GetOrCreate(accessToken, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _timeSpan;
                return new Queue<DateTime>(); // Return a new queue if this is the first request
            });

            if (timestamps == null)
            {
                throw new InvalidOperationException("Timestamps queue is null.");
            }

            return await Task.Run(() => // Task.Run to offload work to a background thread - If queue is large removing timestamps is O(n)
            {
                // Lock list of accessToken timestamps while modifying it - remove timestamps out of range and then check the count
                lock (timestamps)
                {
                    while (timestamps.Count > 0 && timestamps.Peek() < requestTime - _timeSpan)
                    {
                        timestamps.Dequeue();
                    }

                    if (timestamps.Count < _maxRequests) // Allow the request
                    {
                        return true; 
                    }
                }

                return false;
            });
        }

        public Task RecordRequest(string accessToken, DateTime requestTime, Region region = Region.ALL_REGIONS)
        {
            if (_cache.TryGetValue(accessToken, out Queue<DateTime>? timestamps))
            {
                if (timestamps == null)
                {
                    throw new InvalidOperationException("Timestamps queue is null.");
                }

                // Lock the queue while modifying it
                lock (timestamps)
                {
                    timestamps.Enqueue(requestTime);
                }
            }
            else
            {
                throw new InvalidOperationException("Timestamps queue is not found in the cache.");
            }

            return Task.CompletedTask;
        }
    }
}
