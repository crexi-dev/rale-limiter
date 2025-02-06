using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public interface IRateLimiterRule
    {
        Task<bool> EvaluateAsync(HttpContext httpContext);

        string? GetKey(HttpContext context)
        {
            var user = context.User;
            var userId = user?.Identity?.IsAuthenticated == true
                ? user.FindFirst("sub")?.Value
                : null;

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var rateLimitKey = !string.IsNullOrEmpty(userId) ? userId : ipAddress;

            return rateLimitKey;
        }
    }
}