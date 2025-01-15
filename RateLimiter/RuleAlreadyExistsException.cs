using System;

namespace RateLimiter;
public class RuleAlreadyExistsException : Exception
{
    public RuleAlreadyExistsException(Rule rule): base($"The rule with scope {rule.Scope} already exists.")
    {
        
    }
}
