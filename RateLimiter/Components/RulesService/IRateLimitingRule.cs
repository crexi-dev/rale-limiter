using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Components.RuleService
{
    public interface IRateLimitingRule
    {
        Task<bool> RunAsync(RateLimitingRequestData requestData, RateLimitingRuleConfiguration ruleConfig);
    }
}
