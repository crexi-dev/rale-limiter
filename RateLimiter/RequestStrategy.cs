using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
