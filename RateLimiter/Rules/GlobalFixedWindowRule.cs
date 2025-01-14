using System;
using RateLimiter.Interfaces;

namespace RateLimiter
{
    public class GlobalFixedWindowRule : IRateLimitStrategy
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private static readonly object _lock = new object();

        private int _requestCount = 0;
        private DateTime _windowStart = DateTime.UtcNow;

        public GlobalFixedWindowRule(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }

        public bool IsRequestAllowed(string? clientToken)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;

                if ((now - _windowStart) > _window)
                {
                    _windowStart = now;
                    _requestCount = 0;
                }

                if (_requestCount < _limit)
                {
                    _requestCount++;
                    return true;
                }

                return false;
            }
        }
    }
}
