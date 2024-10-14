using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RateLimiterNS.RateLimitRules
{
    public class TimeSpanSinceLastRequestRule : IRateLimitRule
    {
        private readonly TimeSpan _minTimeSpan;
        private readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TimeSpanSinceLastRequestRule(TimeSpan minTimeSpan)
        {
            _minTimeSpan = minTimeSpan;
        }

        public async Task<bool> IsRequestAllowedAsync(string token, DateTime requestTime)
        {
            await _semaphore.WaitAsync();
            try
            {
                var lastRequestTime = _lastRequestTimes.GetValueOrDefault(token);

                if (lastRequestTime == default || requestTime - lastRequestTime >= _minTimeSpan)
                {
                    _lastRequestTimes[token] = requestTime;
                    return true;
                }

                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
