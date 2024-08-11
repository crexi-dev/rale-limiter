using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Interface.Rule;
using RateLimiter.Model;
using System.Collections.Concurrent;

namespace RateLimiter
{
    public class RequestStrategy : Request
    {
        private readonly ILogger<RequestStrategy> _logger;
        public RequestStrategy(ILogger<RequestStrategy> logger) 
        {
            _logger = logger;
        }
        public IEnumerable<IRateLimiterRule>? Rules { get; set; } = null;

        public bool VerifyAccess()
        {
            if (Rules == null)
            {
                _logger.LogWarning("Validation rule not set ");
                return false;
            }

            try
            {
                if (!this.AccessTime.Any())
                {
                    return true;
                }

                var results = new ConcurrentBag<bool>();
                Parallel.ForEach(Rules, rule =>
                {
                    results.Add(rule.VerifyAccess(this));                    
                });

                return results.ToList().All(x => x ==  true);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error with validate access in Validator");
                return false;
            }
            
        }
    }
}
