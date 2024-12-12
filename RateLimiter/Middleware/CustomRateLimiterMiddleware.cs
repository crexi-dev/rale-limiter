using Microsoft.AspNetCore.Http;
using RateLimiter.Models;
using RateLimiter.Persistence;
using RateLimiter.RuleApplicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Middleware;

public class CustomRateLimiterMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IEnumerable<IApplyARateLimit> rateLimiters, IProvideAccessToCachedData repository)
    {
        // Try to get the x-authentication-key header
        if (!context.Request.Headers.TryGetValue("x-authentication-key", out var headerValues))
        {
            // If there is no x-authentication-key in the headers then the request is unauthorized so stop processing this check as well as any future middleware
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var keyValue = headerValues.First();
        var resource = context.Request.Path;

        if (keyValue is null)
        {
            // If there is no x-authentication-key in the headers then the request is unauthorized so stop processing this check as well as any future middleware
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // This applies a separate rate limit per resource; to apply the rate limits only to the client (key) just set `key` to be equal to `keyValue`
        var key = $"{keyValue}_{resource}";

        // Each client/resource combo may have different rules to apply, so fetch those rules from persisted storage
        var ruleConfigurations = repository.GetRuleConfigurationsByKey(key);
        // Each client/resource combo has a dedicated locking resource to avoid multithread collisions where the list of past requests is modified while being evaluated
        repository.Lock(key);
        // Retrieve the list of past requests only once for each client/resource combo so that all past requests can be evaluated by all applicable rules
        var requests = repository.GetRequestsByKey(key);
        bool shouldContinue = true;

        // Iterate the rules and apply them one at a time
        foreach (var ruleConfiguration in ruleConfigurations)
        {
            // Find the (injected) rate limiter that matches the type of the current configuration in the iteration
            var rateLimiter = rateLimiters.FirstOrDefault(rl => rl.Type == ruleConfiguration.Type);

            if (rateLimiter is null)
            {
                // If there is no rate limiter matching the rule then something has gone wrong and there's no way to proceed so stop processing
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                shouldContinue = false;
                break;
            }

            RateLimitResult result;
            try
            {
                // Apply the rate limiter rule
                result = rateLimiter.Apply(ruleConfiguration, requests);
            }
            catch (Exception ex)
            {
                // TODO: log the message somewhere more persisted and valuable than Console
                Console.WriteLine(ex.Message);
                // If something went wrong on our end during the check for rate limiting then assume the rate limit(s) has/have not been met and allow the request to continue
                continue;
            }

            if (!result.IsSuccessful)
            {
                // If the status code is anything other than Success, disallow the request by writing 429 as the status code
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                // Provide more contextual information back to the caller indicating the reason the request was rejected (although 429 is clear, we may be able to tell them to wait x minutes or this is y requests too many in z time frame)
                await context.Response.WriteAsJsonAsync(new { errorMessage = result.Message });
                shouldContinue = false;
                break;
            }
        }

        if (shouldContinue)
        {
            // If none of the rate limit checks caused issues, save this request for future checks to reference
            // Add the request before unlocking the resource to make sure this request is checked for any pending requests after the resource is unlocked
            repository.AddRequestByKey(key);
            // Keep the list of past requests as small as possible each time a new request is received
            repository.RemoveOldRequests(key);
        }

        // Release the lock on this resource for this client so the next request in the queue can be processed
        repository.Unlock(key);

        if (shouldContinue)
        {
            // If none of the rate limit checks caused issues, continue to process additional middleware and the rest of the request
            await _next(context);
        }
    }
}