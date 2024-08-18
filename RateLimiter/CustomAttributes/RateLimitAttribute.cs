using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;

namespace RateLimiter.CustomAttributes;

public class RateLimitAttribute : ActionFilterAttribute
{
    private readonly RateLimitRules[] _inputs;

    public RateLimitAttribute(params RateLimitRules[] inputs)
    {
        _inputs = inputs;
    }
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var service = context.HttpContext.RequestServices.GetService<Func<RateLimitRules, IRateLimitRule>>();

        var userToken = context.HttpContext.Request.Headers["Token"];

        var userInfo = new RateLimitRuleRequestDto
        {
            UserId = 100,
            UserLocal = "EU"
        };
        if (service == null)
        {
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Content = "Rate limit service not available."
            };
            return;
        }

        bool isRateLimited = false;
        foreach (var item in _inputs)
        {
            var ruleService = service(item);
            if (ruleService != null)
            {

                isRateLimited = await ruleService.IsRequestAllowed(userInfo);

            }

        }

        if (isRateLimited)
        {
            context.Result = new ContentResult
            {
                StatusCode = 429,
                Content = "Too many requests. Please try again later."
            };
            return;
        }


        await next();
    }
}
