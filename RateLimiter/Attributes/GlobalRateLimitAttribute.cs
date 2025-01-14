using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter;
using RateLimiter.Rules;

namespace CrexiService.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class GlobalRateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _limit;
        private readonly int _windowSeconds;

        private static readonly object _sync = new object();

        private static readonly Dictionary<(int, int), GlobalFixedWindowRule> _rules
            = new Dictionary<(int, int), GlobalFixedWindowRule>();

        public GlobalRateLimitAttribute(int limit, int windowSeconds)
        {
            _limit = limit;
            _windowSeconds = windowSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var key = (_limit, _windowSeconds);
            GlobalFixedWindowRule rule;

            lock (_sync)
            {
                if (!_rules.TryGetValue(key, out rule))
                {
                    rule = new GlobalFixedWindowRule(_limit, TimeSpan.FromSeconds(_windowSeconds));
                    _rules[key] = rule;
                }
            }

            // ignore token for global rule
            bool allowed = rule.IsRequestAllowed(null); 

            if (!allowed)
            {
                context.Result = new RateLimiter.Attributes.ContentResult
                {
                    StatusCodes = 429,
                    Content = "Too Many Requests - global rate limit exceeded."
                };
                return;
            }

            await next();
        }
    }
}
