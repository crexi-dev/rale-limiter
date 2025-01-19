using Microsoft.AspNetCore.Http;
using RateLimiter.Rules;
using System.Threading.Tasks;

namespace RateLimiter.Middleware
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimiter _rateLimiter;

        public RateLimiterMiddleware(RequestDelegate next, IRateLimiter rateLimiter)
        {
            _next = next;
            _rateLimiter = rateLimiter;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader) && !authorizationHeader.StartsWith("Bearer "))
            {
                return;
            }

            var token = authorizationHeader["Bearer ".Length..].Trim();
            var path = httpContext.Request.Path.Value;

            var isAllowed = _rateLimiter.IsAllowed(token, path);
            if (!isAllowed)
            {
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }

            await _next(httpContext);
        }
    }
}
