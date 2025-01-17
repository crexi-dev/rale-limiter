using RateLimiter.DataStore;
using RateLimiter.Ruls.Abstract;
using RateLimiter.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Ruls
{
    public class MaxRequestAmountInTimeSpanByCountryRule : RateLimiterRuleDecorator
    {
        private readonly TimeSpan _timeFrame;
        private readonly Dictionary<string, int> _maxRequestCountByCountry;
        private readonly int _maxRequestCountDefoult;

        public MaxRequestAmountInTimeSpanByCountryRule(TimeSpan timeFrame, Dictionary<string, int> maxRequestCountByCountry, int maxRequestCountDefoult)
        {
            _timeFrame = timeFrame;
            _maxRequestCountByCountry = maxRequestCountByCountry;
            _maxRequestCountDefoult = maxRequestCountDefoult;
        }

        public override bool IsAllowed(IUserData userData)
        {
            var result = true;
            var key = $"{userData.Token}-MaxRAIT-by-country";
            var storedTimes = Cashing.Get(key);
            if (storedTimes != null)
            {
                var times = storedTimes.Split("_").Select(DateTime.Parse).Where(time => DateTime.Now - time <= _timeFrame).ToList();
                var maxRequestCount = _maxRequestCountByCountry.ContainsKey(key) ? _maxRequestCountByCountry[key] : _maxRequestCountDefoult;
                if (times.Count + 1 <= maxRequestCount)
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
