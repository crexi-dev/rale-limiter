using RateLimiter.Data;
using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models;
using RateLimiter.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class LimiterService : ILimiterService
    {
        protected IDataService<Request> _requestsDataService;
        protected IDataService<Resource> _resourcesDataService;
        protected IDataService<User> _usersDataSource;

        public LimiterService(IDataService<Request> requestsDataService, IDataService<Resource> resourcesDataService, IDataService<User> usersDataSource)
        {
            _requestsDataService = requestsDataService;
            _resourcesDataService = resourcesDataService;
            _usersDataSource = usersDataSource;
        }

        public async Task<bool> AllowAccess(Request request)
        {
            // By default, access allowed to the resource
            bool allowAccess = true;

            var resource = await _resourcesDataService.SingleAsync(request.ResourceId);
            var user = await _usersDataSource.SingleAsync(request.UserId);

            if (resource.LimiterRules == null) 
            {
                allowAccess = true;     // no rules in place so allow access
            }
            if (resource.StatusId == CodeValues.Statuses.Single(x => x.Name == "Offline").Id)
            {
                allowAccess = false;    // resource is offline - unclear if this is the proper place for this logic...
            }

            // Identify the rule to use 
            LimiterRule limiterRule = new LimiterRule { Id = -1, Name = "No rule in effect", NumSeconds = 0, NumPerTimespan = 0 };

            foreach (var rule in resource.LimiterRules)
            {
                if (rule.TokenSource == null || rule.TokenSource == user.TokenSource)
                {
                    limiterRule = rule;
                }
            }

            // If a rule is in effect for the request, evaluate it
            if (limiterRule.Id > 0)
            {
                // if a limiter rule is in effect, then evaluate it here
                var userRequests = await _requestsDataService.FindAsync(new BaseModel { CreatedBy = user.Name });
                var userResourceRequests = userRequests.Where(x => x.ResourceId == resource.Id && x.WasHandled).ToList();

                var numRequests = userResourceRequests.Where(x => x.CreatedDate.AddSeconds(limiterRule.NumSeconds) > DateTime.Now).Count();

                if (numRequests > limiterRule.NumPerTimespan)
                {
                    allowAccess = false;
                }
            }

            // Record the request
            request.WasHandled = allowAccess;
            await _requestsDataService.AddAsync(request);

            return allowAccess;
        }
    }
}
