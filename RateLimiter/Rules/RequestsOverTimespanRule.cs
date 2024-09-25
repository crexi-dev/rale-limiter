using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

/// The `RequestsOverTimespanRule` class is an implementation of the `IRateRule` interface.
/// It evaluates the number of requests made within a specified timespan and determines whether the requests are allowed or not.
/// This rule evaluates whether the number of requests made within a specified timespan is below a certain threshold.
/// If the number of requests exceeds the threshold, the rule will not allow further requests until the timespan has elapsed.
/// This rule takes into account the following parameters:
/// - `allowedRequests`: The maximum number of requests allowed within the timespan. If not specified, it defaults to 0.
/// - `allowedSpanSeconds`: The length of the timespan in seconds. If not specified, it defaults to 0.
/// - `ruleName`: An optional name for the rule. If not specified, it defaults to an empty string.
/// The rule can be overridden by providing a custom implementation of the `Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>` delegate through the `Override` property.
/// Example usage:
/// ```csharp
/// var rule = new RequestsOverTimespanRule(100, 60, "MyRule");
/// var result = await rule.Evaluate(requestDetailsEnumerable);
/// ```
/// @implements IRateRule
/// @typeparam AllowRequestResult The type of the result when evaluating whether a request is allowed or not.
/// @typeparam RequestDetails The type of the request details.
/// @see IRateRule
/// @see AllowRequestResult
/// @see RequestDetails
/// /
public class RequestsOverTimespanRule:IRateRule
{
    private int requestsOverTimespan =
        int.Parse(Environment.GetEnvironmentVariable("RequestsOverTimespan.Count") ?? "10");
    private int allowedSpanInSeconds = int.Parse(Environment.GetEnvironmentVariable("RequestsOverTimespan.SpanInSeconds") ?? "2");
    private readonly string _ruleName;

    /// <summary>
    /// Represents a rate limiting rule that limits the number of requests allowed over a specified timespan.
    /// </summary>
    /// <remarks>
    /// This rule allows you to set the maximum number of requests allowed over a specific timespan.
    /// The rule checks the number of requests made within the specified timespan and blocks the requests if the limit is exceeded.
    /// </remarks>
    /// <example>
    /// This example demonstrates how to create a RequestsOverTimespanRule and configure the allowed requests and timespan:
    /// <code>
    /// RequestsOverTimespanRule rule = new RequestsOverTimespanRule(100, 60, "MyRule");
    /// </code>
    /// </example>
    public RequestsOverTimespanRule(int allowedRequests = 0, int allowedSpanSeconds = 0, string ruleName="")
    {
        _ruleName = ruleName;
        if(allowedRequests > 0)
            requestsOverTimespan = allowedRequests;
        if(allowedSpanSeconds > 0)
            allowedSpanInSeconds = allowedSpanSeconds;
    }

    /// <summary>
    /// Evaluates the rate limiting rule based on the given context.
    /// </summary>
    /// <param name="context">The collection of request details.</param>
    /// <returns>The result of the rate limiting evaluation.</returns>
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        try
        {
            var requestDetailsEnumerable = context as RequestDetails[] ?? context.ToArray();
            if (requestDetailsEnumerable.Count() < requestsOverTimespan)
                return new AllowRequestResult(true, "nice and slow");
        
            var orderedRequests = requestDetailsEnumerable.OrderByDescending(r => r.RequestTime);
        
            var recentRequests = orderedRequests.Take(requestsOverTimespan).ToArray();
            if((recentRequests.Length < requestsOverTimespan) || recentRequests[recentRequests.Length-1].RequestTime.ToUniversalTime().Ticks < DateTime.Now.ToUniversalTime().AddSeconds(-allowedSpanInSeconds).Ticks)
                return new AllowRequestResult(true, "nice and slow");
            return new AllowRequestResult(false, "too many requests");
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }

    /// <summary>
    /// Returns a string representation of the RequestsOverTimespanRule object.
    /// </summary>
    /// <returns>A string that represents the RequestsOverTimespanRule object.</returns>
    public override string ToString()
    {
        return $"{nameof(RequestsOverTimespanRule)}::{_ruleName}";
    }
}