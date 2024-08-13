using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RequestLimitValidator : IRequestLimitValidator
    {        
        private readonly ILogger<IRequestLimitValidator> _logger;
        private readonly IRateLimiterRegionRuleService _rateLimiterRegionRuleService;

        public RequestLimitValidator(ILogger<RequestLimitValidator> logger, IRateLimiterRegionRuleService rateLimiterRegionRuleService)
        {            
            _logger = logger;
            _rateLimiterRegionRuleService = rateLimiterRegionRuleService;
        }
        public bool Validate(Request request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Region))
                {
                    throw new Exception("Request Region not set");
                }

                var rules = _rateLimiterRegionRuleService.GetRulesByRegion(request.Region);
                
                if(rules == null) 
                {
                    throw new Exception("No rules apply this region");
                }

                var results = new ConcurrentBag<bool>();
                Parallel.ForEach(rules, rule =>
                {
                    results.Add(rule.VerifyAccess(request));
                });

                return results.ToList().All(x => x == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validate access at Access Validator");
                return false;
            }
        }
    }
}
