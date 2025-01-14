using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Attributes
{
    public class CompositeRateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private static readonly IUsageRepository _repo = new InMemoryUsageRepository();
        private static readonly CompositeRateLimitStrategy _compositeStrategy;

        static CompositeRateLimitAttribute()
        {
            var strategies = new List<IRateLimitStrategy>
            {
                new FixedWindowRule(limit: 5, window: TimeSpan.FromSeconds(30), _repo),
                new CooldownRule(_repo, TimeSpan.FromSeconds(3))
            };

            _compositeStrategy = new CompositeRateLimitStrategy(strategies);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string clientToken = context.HttpContext.Request.Headers["X-Client-Token"];

            if (string.IsNullOrEmpty(clientToken))
            {
                context.Result = new RateLimiter.Attributes.ContentResult
                {
                    StatusCodes = 400,
                    Content = "client token is required."
                };
                
                return;
            }

            bool allowed = _compositeStrategy.IsRequestAllowed(clientToken);

            if (!allowed)
            {
                context.Result = new RateLimiter.Attributes.ContentResult
                {
                    StatusCodes = 420,
                    Content = "Too Many Requests - rate limit exceeded."
                };

                return;
            }

            await next();
        }
    }
}
