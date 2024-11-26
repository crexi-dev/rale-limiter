using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RateLimiter
{
    public class RejectRateLimiter : IRateLimiter
    {
        private readonly ConcurrentDictionary<string, List<DateTime>> requests = new();
        private readonly ConcurrentDictionary<string, object> tokenLocks = new();

        private readonly object lockObject = new();
        public int RequestLimit { get; }
        public TimeSpan Interval { get; }   
        public RejectRateLimiter(int requestLimit = 10, TimeSpan interval = default)
        { 
            RequestLimit = requestLimit;
            if (interval == default)
                Interval = TimeSpan.FromMinutes(1);
            Interval = interval;
        }
        public bool CheckRequest(string limiterToken)
        {
            var tokenLock = tokenLocks.GetOrAdd(limiterToken, _ => new object());

            lock (tokenLock) // Lock for this specific token
            {
                var currentRequests = requests.GetOrAdd(limiterToken, new List<DateTime>());
                currentRequests = currentRequests
                    .Where(x => x > DateTime.Now.Subtract(Interval))
                    .ToList();

                if (currentRequests.Count < RequestLimit)
                {
                    currentRequests.Add(DateTime.Now);
                    requests[limiterToken] = currentRequests; // Update the dictionary with the filtered list
                    return true;
                }

                return false;
            }
        }
    }
}
