
using RateLimiter.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class XRequestsPerTimespanRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timespan;
        private readonly ConcurrentDictionary<string, List<DateTime>> _requestLogs;

        public XRequestsPerTimespanRule(int maxRequests, TimeSpan timespan)
        {
            _maxRequests = maxRequests;
            _timespan = timespan;
            _requestLogs = new ConcurrentDictionary<string, List<DateTime>>();
        }

        public bool IsRequestAllowed(string clientId, string resource)
        {
            var key = $"{clientId}:{resource}";
            var now = DateTime.UtcNow;

            _requestLogs.TryAdd(key, new List<DateTime>());
            var requestLog = _requestLogs[key];

            lock (requestLog)
            {
                requestLog.RemoveAll(timestamp => (now - timestamp) > _timespan);

                if (requestLog.Count < _maxRequests)
                {
                    requestLog.Add(now);
                    return true;
                }

                return false;
            }
        }

        public bool IsRequestAllowed(string clientId, string resource, string ip)
        {

            return this.IsRequestAllowed(clientId, resource, string.Empty);
        }
    }
}
