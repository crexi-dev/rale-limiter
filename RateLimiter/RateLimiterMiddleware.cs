using Microsoft.AspNetCore.Http;
using RateLimiter.Data.Interfaces;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RequestLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILimiterService _limiterService;
        private readonly IDataService<Resource> _resourcesDataService;
        private readonly IDataService<User> _userDataService;

        public RequestLimiterMiddleware(RequestDelegate next, ILimiterService limiterService, IDataService<Resource> resourcesDataService, IDataService<User> usersDataService)
        {
            _next = next;
            _limiterService = limiterService;
            _resourcesDataService = resourcesDataService;
            _userDataService = usersDataService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var resourceName = context.Request.Path;
            var resource = await _resourcesDataService.SingleAsync(resourceName);  // assumes Identifier matches the pathname of the resource

            var jeff = context.Request.Headers; // Need to pull token from header and match to user token
            var username = "jeff";

            var user = await _userDataService.SingleAsync(username);

            var request = new Request
            {
                Identifier = Guid.NewGuid().ToString(),
                RequestDate = DateTime.Now,
                UserId = user.Id,
                User = user,
                ResourceId = resource.Id,
                Resource = resource ,
                CreatedBy = "jeff",
                CreatedDate = DateTime.UtcNow
            };

            try
            {
                var allowAccess = await _limiterService.AllowAccess(request);

                if (allowAccess)
                {
                    await _next(context); // Call the next middleware component in the pipeline.
                }
            }
            catch
            {
                throw; // Ensure exceptions are propagated.
            }
        }
    }
}