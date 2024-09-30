using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RateLimitRuleA : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeSpan;
        private readonly Dictionary<string, List<DateTime>> _requestLog = new();

        public RateLimitRuleA(int maxRequests, TimeSpan timeSpan)
        {
            _maxRequests = maxRequests;
            _timeSpan = timeSpan;
        }

        public bool IsRequestAllowed(RateLimitContext context)
        {
            // Initialize request log for the client if not present
            if (!_requestLog.ContainsKey(context.ClientToken))
            {
                _requestLog[context.ClientToken] = new List<DateTime>();
            }

            var requests = _requestLog[context.ClientToken];
            var now = context.RequestTime;

            // Remove outdated requests from the log
            requests.RemoveAll(r => r <= now - _timeSpan);

            // Check if the number of requests exceeds the limit
            if (requests.Count >= _maxRequests)
            {
                return false;
            }

            // Add current request to the log
            requests.Add(now);
            return true;
        }
    }

    public class RateLimitRuleB : IRateLimitRule
    {
        private readonly TimeSpan _minTimeSpan;
        private readonly Dictionary<string, DateTime> _lastRequestTime = new();

        public RateLimitRuleB(TimeSpan minTimeSpan)
        {
            _minTimeSpan = minTimeSpan;
        }

        public bool IsRequestAllowed(RateLimitContext context)
        {
            // Initialize last request time for the client if not present
            if (!_lastRequestTime.ContainsKey(context.ClientToken))
            {
                _lastRequestTime[context.ClientToken] = DateTime.MinValue;
            }

            var lastRequest = _lastRequestTime[context.ClientToken];
            var now = context.RequestTime;

            // Check if the time since the last request is less than the minimum timespan
            if (now - lastRequest < _minTimeSpan)
            {
                return false;
            }

            // Update last request time
            _lastRequestTime[context.ClientToken] = now;
            return true;
        }
    }

}
