using RateLimiter.Components.Repository.Models;
using System;

namespace RateLimiter.Components.RuleService.Rules.RuleNRequestPerTimerange
{
    public class RuleNRequestPerTimerangeState : RateLimitingBaseDataRepositoryState
    {
        public DateTime Timestamp { get; set; }
        public int Counter { get; set; }
    }
}
