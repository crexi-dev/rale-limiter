using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.RateLimiters
{
    [Obsolete("Static rule.")]
    public class FixedWindowRateLimiter : IRateLimiter
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private readonly Dictionary<string, (DateTime, int)> _clientRequests = new Dictionary<string, (DateTime, int)>();

        public FixedWindowRateLimiter(int maxRequests, TimeSpan timeWindow)
        {
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
        }

        public bool IsRequestAllowed(string clientId)
        {
            if (_clientRequests.TryGetValue(clientId, out var requestInfo))
            {
                var (startTime, requestCount) = requestInfo;
                if ((DateTime.Now - startTime) < _timeWindow)
                {
                    if (requestCount < _maxRequests)
                    {
                        _clientRequests[clientId] = (startTime, requestCount + 1);
                        return true;
                    }
                    return false;
                }
            }
            _clientRequests[clientId] = (DateTime.Now, 1);
            return true;
        }
    }
}
