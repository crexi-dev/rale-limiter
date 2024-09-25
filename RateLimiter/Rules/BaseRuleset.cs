using System.Collections.Generic;

namespace RateLimiter.Rules;

public abstract class BaseRuleset : List<IRateRule>
{
    protected BaseRuleset()
    {
        Add(new BooleanRule("blarg"));
        Add(new BooleanRule("blink"));
        Add(new TrueRule("blink"));
    }
}