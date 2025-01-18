using System;
using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class FixedCountPerTimeSpanRule : IRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeSpan;
        private readonly ConcurrentDictionary<string, (int requestCount, DateTime windowStart)> _clientRequests = new();

        public FixedCountPerTimeSpanRule(int maxRequests, TimeSpan timeSpan)
        {
            _maxRequests = maxRequests;
            _timeSpan = timeSpan;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            var key = $"{clientId}:{resource}";
            var currentTime = DateTime.UtcNow;

            if (!_clientRequests.TryGetValue(key, out var entry))
            {
                _clientRequests[key] = (1, currentTime);
                return true;
            }

            var (count, windowStart) = entry;

            if (currentTime - windowStart > _timeSpan)
            {
                _clientRequests[key] = (1, currentTime);
                return true;
            }

            if (count < _maxRequests)
            {
                _clientRequests[key] = (count + 1, windowStart);
                return true;
            }

            return false;
        }
    }
}
