using System;
using System.Collections.Generic;
using RateLimiter.Contracts;
using RateLimiter.Rules;

namespace RateLimiter.Attributes;

public class RateLimiterDefaults : IRulesProcessingDefaults<AllowRequestResult, RequestDetails>
{
    public RuleProcessingBehaviors DefaultBehavior { get; set; }
    public double DefaultBlockExpiresDuration { get; set; }
    public DateTime DefineBlockEndTime(DateTime currentTime)
    {
        return currentTime.AddSeconds(DefaultBlockExpiresDuration);
    }

    public IEnumerable<IContextualRule<AllowRequestResult, RequestDetails>> DefaultRules { get; set; }

    public RateLimiterDefaults()
    {
        DefaultBlockExpiresDuration = 10;
        DefaultRules = new List<IContextualRule<AllowRequestResult, RequestDetails>>()
            { new TrueRule("a true things") };
        DefaultBehavior = RuleProcessingBehaviors.Add;
    }
}