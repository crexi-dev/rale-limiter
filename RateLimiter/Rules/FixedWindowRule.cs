using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules
{
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
}
