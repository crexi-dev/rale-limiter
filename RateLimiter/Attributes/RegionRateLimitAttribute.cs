using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Attributes
{
    public class RegionRateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private static InMemoryUsageRepository _repo = new InMemoryUsageRepository();

        private readonly int _usLimit;
        private readonly int _usWindowSeconds;
        private readonly int _euCooldownSeconds;

        public RegionRateLimitAttribute(int usLimit, int usWindowSeconds, int euCooldownSeconds)
        {
            _usLimit = usLimit;
            _usWindowSeconds = usWindowSeconds;
            _euCooldownSeconds = euCooldownSeconds;
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

            IRateLimitStrategy strategy;

            // real world: you would probably use whatever region specifier the cloud provider the service is hosted in injects into the headers
            if (clientToken.StartsWith("US-"))
            {
                strategy = new FixedWindowRule(_usLimit, TimeSpan.FromSeconds(_usWindowSeconds), _repo);
            }
            else if (clientToken.StartsWith("EU-"))
            {
                strategy = new CooldownRule(_repo, TimeSpan.FromSeconds(_euCooldownSeconds));
            }
            else
            {
                strategy = new FixedWindowRule(1, TimeSpan.FromSeconds(10), _repo);
            }

            bool allowed = strategy.IsRequestAllowed(clientToken);

            if (!allowed)
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
