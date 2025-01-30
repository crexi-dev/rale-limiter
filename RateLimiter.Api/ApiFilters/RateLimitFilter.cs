using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Api.Attributes;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Api.ApiFilters;

public class RateLimitFilter(IRateLimitExecutor rateLimitExecutor) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ruleAttribute = context.ActionDescriptor.EndpointMetadata.OfType<RateLimitRuleAttribute>().FirstOrDefault();
        if (ruleAttribute is null || ruleAttribute.RuleTypes.Length == 0)
        {
            await next();
            return;
        }

        var headers = context.HttpContext.Request.Headers;
        
        if (!TryGetHeaderValue(headers, "Identifier", out var identifier) ||
            !Guid.TryParse(identifier, out var parsedIdentifier))
        {
            context.Result = CreateBadRequestContentResult("Missing or invalid required header: Identifier");
            return;
        }

        if (!TryGetHeaderValue(headers, "RegionType", out var regionType) ||
            !Enum.TryParse<RegionType>(regionType, out var parsedRegionType))
        {
            context.Result = CreateBadRequestContentResult("Missing or invalid required header: RegionType");
            return;
        }
        
        var isValid = rateLimitExecutor.Execute(
            ruleAttribute.RuleTypes,
            new Request(parsedIdentifier, parsedRegionType, DateTime.UtcNow));

        if (!isValid)
        {
            context.Result = new ContentResult { StatusCode = StatusCodes.Status429TooManyRequests };
            return;
        }

        await next();
    }
    
    private static bool TryGetHeaderValue(IHeaderDictionary headers, string key, out string value)
    {
        if (headers.TryGetValue(key, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
        {
            value = headerValue.ToString();
            return true;
        }

        value = string.Empty;
        return false;
    }
    
    private static ContentResult CreateBadRequestContentResult(string message) =>
        new() { StatusCode = StatusCodes.Status400BadRequest, Content = message };
}