using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;
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
