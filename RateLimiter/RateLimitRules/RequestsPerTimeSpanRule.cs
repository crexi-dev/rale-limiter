using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiterNS.RateLimitRules
{
    public class RequestsPerTimeSpanRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timespan;
        private readonly ConcurrentDictionary<string, List<DateTime>> _requestTimes = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public RequestsPerTimeSpanRule(int maxRequests, TimeSpan timespan)
        {
            _maxRequests = maxRequests;
            _timespan = timespan;
        }

        public async Task<bool> IsRequestAllowedAsync(string token, DateTime requestTime)
        {
            var times = _requestTimes.GetOrAdd(token, _ => new List<DateTime>());

            await _semaphore.WaitAsync();
            try
            {

                times.RemoveAll(t => t < requestTime - _timespan);


                if (times.Count >= _maxRequests)
                {
                    return false;
                }


                times.Add(requestTime);
                return true;

            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

}
