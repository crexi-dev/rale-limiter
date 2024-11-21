using Microsoft.AspNetCore.Http;
using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models;
using RateLimiter.Interfaces;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace RateLimiter
{
    // this is the class that will be used in an ASP.NET Core Middleware pipeline 
    // allow access or not based on the logic in the LimiterService.  Not sure how to test it.

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
            var resource = await GetResource(context.Request.Path);
            var user = await GetUser();

            var request = CreateRequest(resource, user);

            try
            {
                var proceedToNext = await _limiterService.AllowAccess(request);

                if (proceedToNext)
                {
                    await _next(context); // If the limiter allows access, then call the next middleware component in the pipeline.
                }
            }
            catch
            {
                throw; // Ensure exceptions are propagated.
            }
        }
        protected async Task<Resource> GetResource(string resourceName)
        {
            var resource = await _resourcesDataService.SingleAsync(resourceName);  // assumes Identifier matches the pathname of the resource

            return resource;
        }
        protected async Task<User> GetUser()
        {
            // Identify the user.  A token must exist in the Bearer header of the context.Request.Headers collection.
            // Need to write a service to handle this part.

            var userToken = "[Token pulled from header]"; // context.Request.Headers; 
            var searchCriteria = new BaseModel
            {
                Identifier = userToken
            };
            var users = await _userDataService.FindAsync(searchCriteria);
            if (users == null)
            {
                throw new AuthenticationException("Unknown user.  Please authenticate.");
            }
            if (users.Count > 1)
            {
                throw new AuthenticationException("Multiple users found with the same authentication token.  This should be impossible.");
            }

            var user = users.Single();

            return user;
        }
        protected Request CreateRequest(Resource resource, User user)
        {
            var request = new Request
            {
                Identifier = Guid.NewGuid().ToString(),
                RequestDate = DateTime.Now,
                UserId = user.Id,
                User = user,
                ResourceId = resource.Id,
                Resource = resource,
                CreatedBy = user.Name,
                CreatedDate = DateTime.UtcNow
            };

            return request;
        }
    }
}