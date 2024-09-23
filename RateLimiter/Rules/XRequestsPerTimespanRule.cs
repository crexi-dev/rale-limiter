using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class XRequestsPerTimespanRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private readonly Dictionary<string, (DateTime, int)> _clientRequestTracker;

        public XRequestsPerTimespanRule(int maxRequests, TimeSpan timeWindow)
        {
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
            _clientRequestTracker = new Dictionary<string, (DateTime, int)>();
        }

        public bool IsRequestAllowed(string clientId)
        {
            if (!_clientRequestTracker.TryGetValue(clientId, out var entry))
            {
                _clientRequestTracker[clientId] = (DateTime.Now, 1);
                return true;
            }

            var (startTime, requestCount) = entry;
            if ((DateTime.Now - startTime) > _timeWindow)
            {
                // Reset the time window and request count
                _clientRequestTracker[clientId] = (DateTime.Now, 1);
                return true;
            }
            else if (requestCount < _maxRequests)
            {
                _clientRequestTracker[clientId] = (startTime, requestCount + 1);
                return true;
            }

            return false; // Deny the request if over the limit
        }
    }

}
