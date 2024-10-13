using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiterNS.RateLimitRules
{
    public class TimeSpanSinceLastRequestRule : IRateLimitRule
    {
        private readonly TimeSpan _minTimeSpan;
        private readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();

        public TimeSpanSinceLastRequestRule(TimeSpan minTimeSpan)
        {
            _minTimeSpan = minTimeSpan;
        }

        public bool IsRequestAllowed(string token, DateTime requestTime)
        {
            var lastRequestTime = _lastRequestTimes.GetValueOrDefault(token);
            lock (_lastRequestTimes)
            {
                if (lastRequestTime == default || requestTime - lastRequestTime >= _minTimeSpan)
                {
                    _lastRequestTimes[token] = requestTime;
                    return true;
                }

                return false;
            }
        }
    }
}
