using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// <summary>
/// Represents a rule that combines two rules using a logical OR operation.
/// </summary>
public class OrRule : Decorator
{
    private readonly IRateRule _orRule;
    private readonly string _ruleName;
    public sealed override Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// OrRule(IRateRule rule, IRateRule orRule)
    public OrRule(IRateRule rule, IRateRule orRule) : base(rule)
    {
        _orRule = orRule;
    }


    /// <summary>
    /// Implementation of a rule that allows the evaluation of two rules using a logical OR operation.
    /// </summary>
    public OrRule(IRateRule rule, Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> andRule, string ruleName ="") : base(rule)
    {
        _ruleName = ruleName;
        Override = andRule;
    }

    /// <summary>
    /// Represents a decorator that implements the logical OR operation between two rate rules.
    /// </summary>
    public override async Task<AllowRequestResult>  Evaluate(IEnumerable<RequestDetails> context)
    {
        AllowRequestResult? thisResult = null;
        try
        {
            var baseResult = await base.Evaluate(context);
        
            if(_orRule != null)
                thisResult = await _orRule.Evaluate(context);
            else if(Override != null)
                thisResult = await Override(context);
        
            return new AllowRequestResult((baseResult.AllowRequest || thisResult.AllowRequest),
                $"{baseResult.Reason} -> {thisResult.Reason}");
        }
        catch (Exception e)
        {
            throw;
        }
        
        

    }

    /// <summary>
    /// Overrides the default ToString method of the object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{nameof(OrRule)}::{_ruleName}";
    }
}