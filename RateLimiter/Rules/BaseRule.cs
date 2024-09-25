using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// <summary>
/// Represents a base implementation of the IRateRule interface.
/// </summary>
public class BaseRule:IRateRule
{
    private string RuleName { get; set; }
    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// <summary>
    /// Represents a base rule for rate limiting.
    /// </summary>
    internal BaseRule(Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> test, string ruleName="BaseRuleFromTest")
    {
        RuleName = ruleName;
        Override = test;
    }


    /// <summary>
    /// Evaluates a rate rule by invoking the overridden test.
    /// </summary>
    /// <param name="context">The collection of request details.</param>
    /// <returns>An object of type AllowRequestResult.</returns>
    /// <exception cref="Exception">Thrown when the rule evaluation fails.</exception>
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        try
        {
            return await Override(context);
        }
        catch (Exception ex)
        {
            throw new Exception($"{RuleName} Rule Failed", ex);
        }
    }

    /// <summary>
    /// Returns a string representation of the <see cref="BaseRule"/> object.
    /// </summary>
    /// <returns>A string representation of the <see cref="BaseRule"/> object.</returns>
    public override string ToString()
    {
        return $"{nameof(BaseRule)}::{RuleName}";
    }
}