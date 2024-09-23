using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class TimespanSinceLastCallRule : IRateLimitRule
    {
        private readonly TimeSpan _minTimespanBetweenCalls;
        private readonly Dictionary<string, DateTime> _lastRequestTimes;

        public TimespanSinceLastCallRule(TimeSpan minTimespanBetweenCalls)
        {
            _minTimespanBetweenCalls = minTimespanBetweenCalls;
            _lastRequestTimes = new Dictionary<string, DateTime>();
        }

        public bool IsRequestAllowed(string clientId)
        {
            if (_lastRequestTimes.TryGetValue(clientId, out var lastRequestTime) && (DateTime.Now - lastRequestTime) < _minTimespanBetweenCalls)
            {
                return false;
            }

            _lastRequestTimes[clientId] = DateTime.Now;
            return true;
        }
    }

}
