using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// The `IfThenCase` class represents a case in the `IfThenRule` rule. It encapsulates a test condition, a command to execute if the test condition is true, and the request details for evaluation.
/// @param test A function that takes a `RequestDetails` object as a parameter and returns a boolean value representing the test condition.
/// @param command A function that takes a collection of `RequestDetails` objects as a parameter and returns a `Task<AllowRequestResult>` representing the command to execute if the test condition is true. This parameter is optional.
/// @param requestContext The `RequestDetails` object representing the request context for evaluation.
/// /
public record IfThenCase(
    Func<RequestDetails, bool> test,
    Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? command,
    RequestDetails requestContext);

/// The IfThenRule class represents a rule that allows for conditional evaluation and execution of command functions based on a set of predefined cases.
/// /
public class IfThenRule(string ruleName = "") : IRateRule
{
    private readonly List<IfThenCase> _cases = new();
    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// IfThenRule.cs
    /// The IfThenRule class represents a rule that evaluates multiple IfThenCases and determines whether a request should be allowed or not.
    /// Constructor:
    /// - IfThenRule(string ruleName = "")
    /// Initializes a new instance of the IfThenRule class with an optional rule name.
    /// Properties:
    /// - Override: Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>?
    /// Gets or sets a function that can be used to override the default evaluation of the rule.
    /// Methods:   - Evaluate(IEnumerable<RequestDetails> context): Task<AllowRequestResult>
    /// Evaluates the IfThenRule by executing each IfThenCase and determining whether the request should be allowed or not.
    /// Returns an AllowRequestResult that indicates whether the request should be allowed and the reason for the decision.
    /// Example usage:
    /// // Create the IfThenRule
    /// var rule = new IfThenRule();
    /// // Add IfThenCases to the rule
    /// rule.AddCase(new IfThenCase(test1, command1, requestContext1));
    /// rule.AddCase(new IfThenCase(test2, command2, requestContext2));
    /// // Evaluate the rule
    /// var result = await rule.Evaluate(context);
    /// /
    protected internal IfThenRule(List<IfThenCase> cases) : this()
    {
        _cases = cases;
    }

    /// <summary>
    /// Evaluates the IfThenRule against the given request context.
    /// </summary>
    /// <param name="context">The list of request details.</param>
    /// <returns>An instance of AllowRequestResult indicating whether the request is allowed and the reason.</returns>
    /// <exception cref="System.Exception">Thrown when an exception occurs during evaluation.</exception>
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        List<bool> testResults = [];

        try
        {
            _cases.ForEach(async c =>
            {
                if (c.test(c.requestContext) == true)
                {
                    var result = await c.command!.Invoke(context);
                    testResults.Add(result.AllowRequest);
                }
            });

            var allPass = testResults.All(x => true);
            return new AllowRequestResult(allPass, "not all the tests passed");
        }
        catch (Exception e)
        {
            throw;
        }
    }


    /// <summary>
    /// Returns a string representation of the <see cref="IfThenRule"/> instance.
    /// </summary>
    /// <returns>A string representing the <see cref="IfThenRule"/>.</returns>
    public override string ToString()
    {
        return $"{nameof(IfThenRule)}::{ruleName}";
    }
}