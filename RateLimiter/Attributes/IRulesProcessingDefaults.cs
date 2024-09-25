using System;
using System.Collections.Generic;
using RateLimiter.Rules;

namespace RateLimiter.Attributes;

public interface IRulesProcessingDefaults<TRuleResultType, TRuleContext>
{
    RuleProcessingBehaviors DefaultBehavior { get; set; }
    double DefaultBlockExpiresDuration { get; set; }
    DateTime DefineBlockEndTime(DateTime currentTime);
    IEnumerable<IContextualRule<TRuleResultType, TRuleContext>> DefaultRules { get; set; }
}

public enum RuleProcessingBehaviors
{
    Add,
    Override
}
