using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.RateLimiters
{
    [Obsolete("Static rule.")]
    public class SlidingWindowRateLimiter : IRateLimiter
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private readonly Dictionary<string, Queue<DateTime>> _clientRequestTimes = new Dictionary<string, Queue<DateTime>>();

        public SlidingWindowRateLimiter(int maxRequests, TimeSpan timeWindow)
        {
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
        }

        public bool IsRequestAllowed(string clientId)
        {
            if (!_clientRequestTimes.ContainsKey(clientId))
            {
                _clientRequestTimes[clientId] = new Queue<DateTime>();
            }

            var requestTimes = _clientRequestTimes[clientId];
            var now = DateTime.Now;

            while (requestTimes.Count > 0 && (now - requestTimes.Peek()) > _timeWindow)
            {
                requestTimes.Dequeue();
            }

            if (requestTimes.Count < _maxRequests)
            {
                requestTimes.Enqueue(now);
                return true;
            }

            return false;
        }
    }

}
