using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Policy;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Usage
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RateLimitAttribute(string policyName) : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var serviceProvider = context.HttpContext.RequestServices;

            var policyRegistry = serviceProvider.GetRequiredService<RateLimitPolicyRegistry>();
            var policy = policyRegistry.GetPolicy(policyName);
            if (policy == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            if (!await policy.EvaluateAllAsync(context.HttpContext))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 429,
                    Content = "Rate limit exceeded. Try again later."
                };
                return;
            }

            await next();
        }
    }
}
