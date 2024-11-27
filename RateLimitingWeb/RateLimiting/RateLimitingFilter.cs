using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Components.RuleService;
using RateLimiter.Models;

namespace RateLimiterWeb.RateLimiting
{
    public class RateLimitingFilter : IAsyncActionFilter
    {
        private readonly IRateLimitingService _rateLimitingService;

        public RateLimitingFilter(IRateLimitingService rateLimitingService) 
        { 
            _rateLimitingService = rateLimitingService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;

                RateLimitingRequestData requestData = new()
                {
                    Ip = context.HttpContext.Connection.RemoteIpAddress,
                    Action = descriptor.ActionName,
                    Controller = descriptor.ControllerName,
                    Parameters = descriptor.Parameters.Select(item => item.Name).ToList()

                    // this section can grow extracting more information from the request as needed. ex. headers, etc
                };

                // get groups from attributes (controllers and actions)
                var groups = context
                    .ActionDescriptor
                    .EndpointMetadata
                    .Where(item => item is RateLimitingAttribute)
                    .Select(item => ((RateLimitingAttribute)item).GroupName)
                    .ToList();


                if (!await _rateLimitingService.CanProcessRequestAsync(requestData, groups))
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluating rate limiting: {ex.ToString()}");

                // we don't want to prevent clients from working if there is an error in our side
            }

            await next();
        }
    }
}
