using RateLimiter.Rules.Models;
using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules
{
    public class RequestPerTimeSpanRule : IRateLimitRule
    {
        private readonly ConcurrentDictionary<string, FixedTimeWindowLimitTracker> _userRequestLimits;
        private readonly TimeSpan _interval;
        private readonly uint _numberOfRequests;

        public RequestPerTimeSpanRule(uint numberOfRequests, TimeSpan interval)
        {
            _interval = interval;
            _numberOfRequests = numberOfRequests;
            _userRequestLimits = new ConcurrentDictionary<string, FixedTimeWindowLimitTracker>();
        }

        public bool IsWithinLimit(string userId)
        {
            var currentRequestTime = DateTime.UtcNow.Ticks;
            var limitTracker = _userRequestLimits.GetOrAdd(userId, new FixedTimeWindowLimitTracker(0, currentRequestTime));

            if (IsWithinTimeInterval(currentRequestTime, limitTracker.InitialRequestTime))
            {
                limitTracker.IncrementCount();
            }
            else
            {
                limitTracker.Reset(currentRequestTime);
            }

            if (limitTracker.Count > _numberOfRequests)
            {
                return false;
            }

            return true;
        }

        private bool IsWithinTimeInterval(long currentRequestTime, long initialRequestTime)
        {
            return currentRequestTime - initialRequestTime <= _interval.Ticks;
        }
    }
}
