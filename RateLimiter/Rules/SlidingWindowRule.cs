using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
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
}
