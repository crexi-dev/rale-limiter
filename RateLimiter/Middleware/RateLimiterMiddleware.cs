using RateLimiter.Services;
using System.Text.Json;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimiterManager _rateLimiterManager;

    public RateLimiterMiddleware(RequestDelegate next, RateLimiterManager rateLimiterManager)
    {
        _next = next;
        _rateLimiterManager = rateLimiterManager;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        if (path.StartsWith("/swagger") || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        // Validate X-Client-Id header
        string clientId = context.Request.Headers["X-Client-Id"];
        if (string.IsNullOrEmpty(clientId))
        {
            await RespondWithError(context, 400, "MISSING_CLIENT_ID", "Client ID is required in the 'X-Client-Id' header.");
            return;
        }

        string resource = context.Request.Path.ToString();
        var rateLimitResult = _rateLimiterManager.IsRequestAllowed(clientId, resource);

        if (!rateLimitResult.IsAllowed)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = Math.Ceiling(rateLimitResult.RetryAfter.TotalSeconds).ToString();
            await RespondWithError(context, 429, "RATE_LIMIT_EXCEEDED", "Rate limit exceeded. Try again later.", rateLimitResult.RetryAfter.TotalSeconds);
            return;
        }

        await _next(context);
    }

    private static async Task RespondWithError(HttpContext context, int statusCode, string errorCode, string message, double? retryAfter = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            error = new
            {
                code = errorCode,
                message = message,
                retry_after = Math.Ceiling(retryAfter ?? 0) // Ensures no long decimal places
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
