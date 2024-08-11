using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Interface.Rule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter
{    
    public class RequestLimitValidator : IRequestLimitValidator
    {
        private readonly IEnumerable<IRateLimiterRule> _rules;
        private readonly ILogger<IRequestLimitValidator> _logger;
        
        public RequestLimitValidator(ILogger<RequestLimitValidator> logger,IEnumerable<IRateLimiterRule> rules)
        {
            _rules = rules;        
            _logger = logger;
        }
        public bool Validate(RequestStrategy request) 
        {
            try
            {
                request.Rules = _rules.Where( x => x.SupportedRegion.Contains(request.Region));
                return request.VerifyAccess();
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Error validate access at Access Validator");
                return false;
            }
        }
    }
}
