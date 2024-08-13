using Microsoft.Extensions.Logging;
using RateLimiter.Interface.Rule;
using RateLimiter.Model;
using System.Collections.Concurrent;

namespace RateLimiter
{
    public class RequestStrategy : Request
    {        
        public IEnumerable<IRateLimiterRule>? Rules { get; set; } = null;

        public bool VerifyAccess()
        {
            if (Rules == null)
            {                
                return false;
            }

            if (!this.AccessTime.Any())
            {
                return true;
            }

            var results = new ConcurrentBag<bool>();
            Parallel.ForEach(Rules, rule =>
            {
                results.Add(rule.VerifyAccess(this));
            });

            return results.ToList().All(x => x == true);
        }
    }
}
