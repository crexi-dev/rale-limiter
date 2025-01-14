using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Attributes
{
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private static IUsageRepository _repo = new InMemoryUsageRepository();
        private readonly int _limit;
        private readonly int _windowSeconds;

        public RateLimitAttribute(int limit = 1, int windowSeconds = 10)
        {
            _limit = limit;
            _windowSeconds = windowSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string clientToken = context.HttpContext.Request.Headers["X-Client-Token"];

            if (string.IsNullOrEmpty(clientToken))
            {
                context.Result = new ContentResult
                {
                    StatusCodes = 400,
                    Content = "Client token is required."
                };
                return;
            }

            //IUsageRepository usageRepo = new InMemoryUsageRepository();
            var strategy = new FixedWindowRule(
                limit: _limit,
                window: TimeSpan.FromSeconds(_windowSeconds),
                _repo
            );

            if (!strategy.IsRequestAllowed(clientToken))
            {
                context.Result = new ContentResult
                {
                    StatusCodes = 429,
                    Content = "Too Many Requests - rate limit exceeded."
                };
                return;
            }

            await next();
        }
    }
}
