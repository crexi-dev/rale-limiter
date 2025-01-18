using System;
using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class MinTimeBetweenRequestsRule : IRule
    {
        private readonly TimeSpan _minTimeBetweenRequests;
        private readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();

        public MinTimeBetweenRequestsRule(TimeSpan minTimeBetweenRequests)
        {
            _minTimeBetweenRequests = minTimeBetweenRequests;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            var key = $"{clientId}:{resource}";
            var currentTime = DateTime.UtcNow;

            if (_lastRequestTimes.TryGetValue(key, out var lastRequestTime))
            {
                if (currentTime - lastRequestTime < _minTimeBetweenRequests)
                {
                    return false;
                }
            }

            _lastRequestTimes[key] = currentTime;
            return true;
        }
    }
}
