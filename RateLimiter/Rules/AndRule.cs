using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// <summary>
/// Represents a rule that combines the results of two other rules using the logical "AND" operation.
/// </summary>
/// <remarks>
/// The AndRule class is a decorator class that takes two rule objects and combines their results using the logical "AND" operation.
/// The Evaluate method of the AndRule class evaluates the two rules and returns a combined result.
/// If both rules allow the request, the combined result will allow the request.
/// If either rule denies the request, the combined result will deny the request.
/// The Reason property of the combined result will contain the reasons from both rules, separated by "->".
/// </remarks>
public class AndRule : Decorator
{
    private string RuleName { get; set; }
    private readonly IRateRule? _andRule = null;
    public sealed override Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// The `AndRule` class is a decorator for `IRateRule` instances that combines the functionality of two rate rules
    /// using the logical AND operator.
    /// @namespace RateLimiter.Rules
    /// @inherits RateLimiter.Rules.Decorator
    /// /
    public AndRule(IRateRule rule, IRateRule andRule, string? ruleName="") : base(rule)
    {
        if(ruleName != null)
            RuleName = ruleName;
        _andRule = andRule;
    }


    /// <summary>
    /// Represents a rule that combines two other rules using a logical AND operation.
    /// </summary>
    public AndRule(IRateRule rule, Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> andRule, string? ruleName="") : base(rule)
    {
        if(ruleName != null)
            RuleName = ruleName;
        Override = andRule;
    }

    /// <summary>
    /// Evaluates the AndRule by combining the results of the base rule and the provided additional rule.
    /// </summary>
    /// <param name="context">The collection of RequestDetails objects.</param>
    /// <returns>The result of the evaluation, which includes whether the request is allowed and the reason for the result.</returns>
    /// <exception cref="Exception">Thrown when an exception occurs during evaluation.</exception>
    public override async Task<AllowRequestResult>  Evaluate(IEnumerable<RequestDetails> context)
    {
        try
        {
            AllowRequestResult? thisResult = null;
            var baseResult = await base.Evaluate(context);
            
            if(_andRule != null)
                thisResult = await _andRule.Evaluate(context);
            else if(Override != null)
                thisResult = await Override(context);
            
            return new AllowRequestResult((baseResult.AllowRequest && thisResult.AllowRequest),
                $"{baseResult.Reason} -> {thisResult.Reason}");
        }
        catch (Exception ex)
        {
            throw new Exception(this.ToString(), ex);
        }
    }

    /// <summary>
    /// Returns a string representation of the <see cref="AndRule"/> object.
    /// </summary>
    /// <returns>A string that represents the current <see cref="AndRule"/>.</returns>
    public override string ToString()
    {
        return $"{nameof(AndRule)}::{RuleName}";
    }
}