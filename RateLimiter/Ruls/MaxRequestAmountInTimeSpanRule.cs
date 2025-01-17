using RateLimiter.DataStore;
using RateLimiter.Ruls.Abstract;
using RateLimiter.User;
using System;
using System.Linq;

namespace RateLimiter.Ruls
{
    public class MaxRequestAmountInTimeSpanRule : RateLimiterRuleDecorator
    {
        private readonly TimeSpan _timeFrame;
        private readonly int _maxRequestCount;

        public MaxRequestAmountInTimeSpanRule(TimeSpan TimeFrame, int maxRequestCount)
        {
            _timeFrame = TimeFrame;
            _maxRequestCount = maxRequestCount;
        }

        public override bool IsAllowed(IUserData userData)
        {
            var result = true;
            var key = $"{userData.Token}-MaxRAIT";
            var storedTimes = Cashing.Get(key);
            if (storedTimes != null)
            {
                var times = storedTimes.Split("_").Select(DateTime.Parse).Where(time => DateTime.Now - time <= _timeFrame).ToList();
                if (times.Count + 1 <= _maxRequestCount)
                {
                    times.Add(DateTime.Now);
                    Cashing.Set(key, string.Join("_", times.Select(x => x.ToString())));
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                Cashing.Set(key, DateTime.Now.ToString());
            }
            return result && (RateLimiterRule == null || RateLimiterRule.IsAllowed(userData));
        }
    }
}
