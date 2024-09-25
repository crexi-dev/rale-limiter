using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public abstract class Decorator : IRateRule
{
    private readonly IRateRule _rule;

    protected Decorator(IRateRule rule)
    {
        _rule = rule;
    }
    
    public virtual async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        try
        {
            return await _rule.Evaluate(context);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }
    
    public override string ToString()
    {
        return $"{nameof(Decorator)}::{_rule}";
    }
}

