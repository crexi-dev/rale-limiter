using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter
{    
    public class RateLimiterService : IRateLimiterService
    {
        private readonly IEnumerable<IRateLimiterRule> _rules;
        private readonly ILogger<IRateLimiterService> _logger;
        
        public RateLimiterService(ILogger<RateLimiterService> logger,IEnumerable<IRateLimiterRule> rules)
        {
            _rules = rules;        
            _logger = logger;
        }
        public bool Validate(Request request) 
        {
            try
            {
                var requestValidator = (RequestStrategy)request;
                requestValidator.Rules = _rules.Where(x => x.SupportedRegion.Contains(request.Region));
                return requestValidator.VerifyAccess();
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Error validate access at Access Validator");
                return false;
            }
        }
    }
}
