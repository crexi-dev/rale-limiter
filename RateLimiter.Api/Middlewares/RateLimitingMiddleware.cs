using RateLimiter.Attributes;
using RateLimiter.Rules;
using RateLimiter.Storages;
using System.Collections.Concurrent;

namespace RateLimiter.Api.Middlewares;
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRateLimitStore _store;
    private readonly ConcurrentDictionary<string, IRateLimitRule> _rulesCache = new();

    public RateLimitingMiddleware(RequestDelegate next, IServiceProvider serviceProvider, IRateLimitStore store)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _store = store;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var clientId = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(clientId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Client ID is missing.");
            return;
        }

        var actionKey = endpoint.DisplayName;

        // Get rate limit rules from attributes
        var rateLimitRules = GetRateLimitRules(endpoint);

        if (rateLimitRules.Any())
        {
            var compositeRule = new CompositeRule(rateLimitRules);

            if (!await compositeRule.IsRequestAllowedAsync(clientId, actionKey, _store))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60"; // Example value
                await context.Response.WriteAsync("Rate limit exceeded.");
                return;
            }
        }

        await _next(context);
    }

    private IEnumerable<IRateLimitRule> GetRateLimitRules(Endpoint endpoint)
    {
        var rateLimitAttributes = endpoint.Metadata.GetOrderedMetadata<RateLimitAttribute>();
        var rules = new List<IRateLimitRule>();

        foreach (var attribute in rateLimitAttributes)
        {
            var key = GetAttributeKey(attribute);

            // Use a cache to prevent creating multiple instances of the same rule
            var rule = _rulesCache.GetOrAdd(key, _ =>
            {
                return attribute.CreateRule(_serviceProvider);
            });
            rules.Add(rule);
        }

        return rules;
    }

    private string GetAttributeKey(RateLimitAttribute attribute)
    {
        // Create a unique key based on the attribute's type and properties
        var properties = attribute.GetType().GetProperties()
            .Select(p => p.GetValue(attribute)?.ToString() ?? "");
        return attribute.GetType().FullName + ":" + string.Join(":", properties);
    }
}