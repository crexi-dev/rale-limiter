using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace RateLimiter
{
    public interface IRateLimitRule
    {
        bool IsAllowed(string clientId, string resource);
        void Cleanup();
    }

    public class FixedWindowRule : IRateLimitRule
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, (int count, DateTime resetTime)> _requests = new();

        public FixedWindowRule(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            var key = $"{clientId}:{resource}";
            var now = DateTime.UtcNow;

            _requests.AddOrUpdate(key, _ => (1, now + _window), (_, entry) =>
            {
                if (entry.resetTime <= now)
                    return (1, now + _window);
                return (entry.count + 1, entry.resetTime);
            });

            return _requests[key].count <= _limit;
        }

        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            foreach (var key in _requests.Keys)
            {
                if (_requests.TryGetValue(key, out var entry) && entry.resetTime <= now)
                {
                    _requests.TryRemove(key, out _);
                }
            }
        }
    }

    public class SlidingWindowRule : IRateLimitRule
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new();

        public SlidingWindowRule(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            var key = $"{clientId}:{resource}";
            var now = DateTime.UtcNow;

            _requests.AddOrUpdate(key, _ => new Queue<DateTime>(new[] { now }), (_, queue) =>
            {
                while (queue.Count > 0 && queue.Peek() <= now - _window)
                    queue.Dequeue();
                queue.Enqueue(now);
                return queue;
            });

            return _requests[key].Count <= _limit;
        }

        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            foreach (var key in _requests.Keys)
            {
                if (_requests.TryGetValue(key, out var queue))
                {
                    while (queue.Count > 0 && queue.Peek() <= now - _window)
                        queue.Dequeue();
                }
            }
        }
    }

    public class RegionalRule : IRateLimitRule
    {
        private readonly Dictionary<string, IRateLimitRule> _regionRules;

        public RegionalRule(Dictionary<string, IRateLimitRule> regionRules)
        {
            _regionRules = regionRules;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            var region = GetRegionFromClient(clientId);
            return _regionRules.TryGetValue(region, out var rule) && rule.IsAllowed(clientId, resource);
        }

        private string GetRegionFromClient(string clientId)
        {
            return clientId.StartsWith("US-") ? "US" : "EU";
        }

        public void Cleanup()
        {
            foreach (var rule in _regionRules.Values)
            {
                rule.Cleanup();
            }
        }
    }

    public class RateLimiter
    {
        private readonly ConcurrentDictionary<string, List<IRateLimitRule>> _resourceRules = new();
        private readonly ILogger<RateLimiter> _logger;

        public RateLimiter(ILogger<RateLimiter> logger)
        {
            _logger = logger;
        }

        public void AddRule(string resource, IRateLimitRule rule)
        {
            _resourceRules.AddOrUpdate(resource, _ => new List<IRateLimitRule> { rule }, (_, rules) => { rules.Add(rule); return rules; });
        }

        public bool IsRequestAllowed(string clientId, string resource)
        {
            if (!_resourceRules.TryGetValue(resource, out var rules))
                return true;

            foreach (var rule in rules)
            {
                if (!rule.IsAllowed(clientId, resource))
                {
                    _logger.LogWarning($"Request blocked: Client {clientId}, Resource {resource}");
                    return false;
                }
            }
            return true;
        }

        public void Cleanup()
        {
            foreach (var rules in _resourceRules.Values)
            {
                foreach (var rule in rules)
                {
                    rule.Cleanup();
                }
            }
        }
    }
}
