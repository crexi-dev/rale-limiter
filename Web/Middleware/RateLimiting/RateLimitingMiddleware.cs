namespace Web.Middleware.RateLimiting;

using RateLimiter;

public class RateLimitingMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract token (for example, from query string or headers)
        var token = context.Request.Query["token"].ToString();

        // Determine the resource name (e.g., Weather or News) based on the endpoint path
        var resourceName = context.Request.Path.Value.Contains("weather", StringComparison.OrdinalIgnoreCase) ? "Weather" :
            context.Request.Path.Value.Contains("news", StringComparison.OrdinalIgnoreCase) ? "News" : null;
        
        if (resourceName == null)
        {
            await next(context);
            return;
        }
        
        var rateLimiter = serviceProvider.GetRequiredKeyedService<IRateLimiter>(resourceName);
        
        // Check if the request is allowed
        var (isAllowed, nextAllowedTime) = await rateLimiter.IsRequestAllowedAsync(resourceName, token);
        if (isAllowed)
        {
            // If allowed, continue with the next middleware or controller
            await next(context);
            return;
        }

        var retryAfterSeconds = (nextAllowedTime - DateTime.UtcNow).TotalSeconds;
        context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString("F0"));
        context.Response.Headers.Append("X-RateLimit-Reset", nextAllowedTime.ToUniversalTime().ToString("r"));
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.Response.WriteAsync("Too many requests.");
    }
}
