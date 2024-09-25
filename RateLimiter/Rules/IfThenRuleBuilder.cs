using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// <summary>
/// Adds a rule to the IfThenRuleBuilder.
/// </summary>
/// <param name="test">The delegate that defines the condition for the rule.</param>
/// <param name="command">The delegate that defines the action to be taken if the condition is met.</param>
/// <param name="requestContext">The request context to be passed to the rule.</param>
/// <returns>The IfThenRuleBuilder instance with the added rule.</returns>
public class IfThenRuleBuilder 
{
    List<IfThenCase> cases = new();

    /// <summary>
    /// Adds a rule to the IfThenRuleBuilder.
    /// </summary>
    /// <param name="test">The delegate that defines the condition for the rule.</param>
    /// <param name="command">The delegate that defines the action to be taken if the condition is met.</param>
    /// <param name="requestContext">The request context to be passed to the rule.</param>
    /// <returns>The IfThenRuleBuilder instance with the added rule.</returns>
    public IfThenRuleBuilder WithRule(Func<RequestDetails, bool> test,
        Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> command, RequestDetails requestContext)
    {
        cases.Add(new IfThenCase(test, command, requestContext));
        return this;
    }

    /// <summary>
    /// Builds and returns an instance of IfThenRule.
    /// </summary>
    /// <returns>The IfThenRule instance.</returns>
    public IfThenRule Build()
    {
        var rule = new IfThenRule(cases);
        return rule;
    }
}