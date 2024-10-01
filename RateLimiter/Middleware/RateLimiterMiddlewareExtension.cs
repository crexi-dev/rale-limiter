using Microsoft.AspNetCore.Builder;

namespace RateLimiter.Middleware
{
    public static class RateLimiterMiddlewareExtension
    {
        public static IApplicationBuilder UseRateLimiter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimiterMiddleware>();
        }
    }
}
