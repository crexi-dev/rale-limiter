using RateLimiter.Repositories;
using RateLimiter.Service.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RateLimiter
    {
        public bool IsAllowed(string resource, string token, DateTime requestTime, IEnumerable<IRule> rules)
        {
            var allow = new ConcurrentBag<bool>();

            RequestsData.SaveRequests(resource, token, requestTime);

            Parallel.ForEach(rules, rule =>
            {
                allow.Add(rule.Allow(resource, token, requestTime));
            });

            return allow.Contains(false) ? false : true;
        }
    }
}
