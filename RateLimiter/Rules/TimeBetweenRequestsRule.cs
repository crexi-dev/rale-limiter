using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// <summary>
/// Represents a rule that measures the time between requests.
/// </summary>
public class TimeBetweenRequestsRule : IRateRule
{
    private readonly int defaultTimeBetweenRequests =
        int.Parse(Environment.GetEnvironmentVariable("DefaultTimeBetweenRequests") ?? "5");

    private readonly string _ruleName;

    /// The `TimeBetweenRequestsRule` class is an implementation of the `IRateRule` interface. It represents a rule that evaluates the time duration between consecutive requests and determines whether the requests are allowed based on a predefined time threshold.
    /// This class requires an implementation of the `IContextualRule` interface, which specifies the input type (`RequestDetails`) and the result type (`AllowRequestResult`) for the rule evaluation. It also requires an implementation of the `IOverridableRule` interface, which defines a property for an override function to be used for rule evaluation.
    /// The `TimeBetweenRequestsRule` class has the following properties:
    /// - `defaultTimeBetweenRequests`: A private readonly integer representing the default time threshold between requests. It is initialized with the value retrieved from the "DefaultTimeBetweenRequests" environment variable (or 5 if the variable is not found).
    /// - `_ruleName`: A private string representing the name of the rule.
    /// - `Override`: A nullable delegate that points to an override function to be used for rule evaluation.
    /// The `TimeBetweenRequestsRule` class has the following methods:
    /// - `TimeBetweenRequestsRule`: A constructor that initializes the `_ruleName` property and optionally overrides the `defaultTimeBetweenRequests` property.
    /// - `Evaluate`: An asynchronous method that takes a collection of `RequestDetails` objects as input and evaluates whether the time duration between consecutive requests is greater than or equal to the `defaultTimeBetweenRequests` value. It returns an `AllowRequestResult` object indicating whether the requests are allowed and providing a reason message.
    /// - `ToString`: A method that returns a string representation of the rule in the format "TimeBetweenRequestsRule::{_ruleName}".
    /// See the `IRateRule`, `IContextualRule`, `IOverridableRule`, `RequestDetails`, and `AllowRequestResult` documentation for more information on the related interfaces and classes used by this rule.
    public TimeBetweenRequestsRule(int timeBetweenOverride = 0, string ruleName = "")
    {
        _ruleName = ruleName;

        if (timeBetweenOverride > 0)
            defaultTimeBetweenRequests = timeBetweenOverride;
    }

    /// <summary>
    /// Evaluates the time between requests rule to determine whether a request is allowed based on the time elapsed between the most recent request and the second most recent request.
    /// </summary>
    /// <param name="context">The collection of request details.</param>
    /// <returns>An instance of AllowRequestResult representing whether the request is allowed and the reason for the decision.</returns>
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        try
        {
            if (context.Count() < 2)
                return new AllowRequestResult(true, "Only one request has been made");

            var ordered = context.OrderByDescending(r => r.RequestTime).ToArray();

            var mostRecent = ordered[0];
            var lastRequest = ordered[1];

            var timeBetweenRequests = (mostRecent.RequestTime - lastRequest.RequestTime).TotalSeconds;

            var allowed = timeBetweenRequests >= defaultTimeBetweenRequests;
            var message = allowed ? "good job" : "slow down!";

            return new AllowRequestResult(allowed, message);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// <summary>
    /// Returns a string representation of the rule in the format "TimeBetweenRequestsRule::{_ruleName}".
    /// </summary>
    public override string ToString()
    {
        return $"{nameof(TimeBetweenRequestsRule)}::{_ruleName}";
    }
}