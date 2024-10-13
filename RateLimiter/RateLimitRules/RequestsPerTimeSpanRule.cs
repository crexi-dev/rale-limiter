using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiterNS.RateLimitRules
{
    public class RequestsPerTimeSpanRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timespan;
        private readonly ConcurrentDictionary<string, List<DateTime>> _requestTimes = new();

        public RequestsPerTimeSpanRule(int maxRequests, TimeSpan timespan)
        {
            _maxRequests = maxRequests;
            _timespan = timespan;
        }

        public bool IsRequestAllowed(string token, DateTime requestTime)
        {
            var times = _requestTimes.GetOrAdd(token, _ => new List<DateTime>());
            lock (times) //locking for safe concurrent access
            {
                times.RemoveAll(t => t < requestTime - _timespan);

                if (times.Count >= _maxRequests)
                {
                    return false;
                }

                times.Add(requestTime);
                return true;
            }
        }
    }
}
