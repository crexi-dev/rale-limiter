using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// The BlockAfterFiveRequestsRule class is a rate rule that blocks requests after a certain number of requests.
/// It implements the IRateRule interface.
/// @constructor BlockAfterFiveRequestsRule
/// @param {string} ruleName - The name of the rule.
/// @property {Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>} [Override] - An optional override function that can be used to customize the evaluation logic of the rule.
/// @method Evaluate - Evaluates the rule based on the provided context of request details and returns an AllowRequestResult.
/// * @param {IEnumerable<RequestDetails>} context - The context of request details.
/// * @returns {Task<AllowRequestResult>} - A task that represents the asynchronous evaluation operation.
/// @method ToString - Returns a string representation of the BlockAfterFiveRequestsRule.
/// * @returns {string} - A string representation of the BlockAfterFiveRequestsRule.
/// /
public class BlockAfterFiveRequestsRule(string ruleName) : IRateRule
{
    /// Evaluates the given request context and determines if the request is allowed or not.
    /// @param context The request context as a collection of RequestDetails objects.
    /// @return An AllowRequestResult object representing the result of the evaluation.
    /// The AllowRequest propery indicates whether the request is allowed or not.
    /// The Reason property provides a reason for the evaluation result.
    /// /
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        try
        {
            return await Task.Run(() => context.ToList().Count == 5 ? new AllowRequestResult(false, "too many requests") : new AllowRequestResult(true, "all good"));
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// <summary>
    /// Overrides the ToString method to provide custom string representation of the BlockAfterFiveRequestsRule instance.
    /// </summary>
    /// <returns>A string representing the BlockAfterFiveRequestsRule instance.</returns>
    public override string ToString()
    {
        return $"{nameof(BlockAfterFiveRequestsRule)}::{ruleName}";
    }
}