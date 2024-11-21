using RateLimiter.Data;
using RateLimiter.Data.CodeValues;
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
            // By default, access is denied to the resource
            bool allowAccess = false;

            var resource = await _resourcesDataService.SingleAsync(request.ResourceId);
            var user = await _usersDataSource.SingleAsync(request.UserId);

            if (resource.LimiterRules == null || resource.LimiterRules.Count == 0) 
            {
                allowAccess = true;     // no rules in place so allow access
            }
            if (resource.StatusId == Statuses.Offline.Id)
            {
                allowAccess = false;    // resource is offline - unclear if this is the proper place for this logic...
            }

            // Identify the rule to use - the first one in the LimiterRules collection that evaluates to true will be the one in effect
            LimiterRule limiterRule = new LimiterRule { Id = -1, Name = "No rule in effect", NumSeconds = 0, NumPerTimespan = 0 };

            foreach (var rule in resource.LimiterRules)
            {
                if (rule.IsPriorityUser == true && user.IsPriorityUser)
                {
                    limiterRule = rule;
                    break;
                }
                else if (rule.TokenSource != null && rule.TokenSource == user.TokenSource)  // does user token match the rule
                {
                    limiterRule = rule;
                    break;
                }
                else if (rule.ResourceStatusId == Statuses.Maintenance.Id && resource.StatusId == Statuses.Maintenance.Id)
                {
                    limiterRule = rule;
                    break;
                }
            }

            // If a rule is in effect for the request, evaluate it
            if (limiterRule.Id > 0)
            {
                // All rules are based on the number of requests that were allowed in the past so 
                // so they must be retrieved.  I did not implement a Search() method for the 
                // Requests collections - only a Find() which uses the base model to specify search criteria.
                var userRequests = await _requestsDataService.FindAsync(new BaseModel { CreatedBy = user.Name }); 
                var userResourceRequests = userRequests.Where(x => x.ResourceId == resource.Id && x.WasHandled == true).ToList();

                if (limiterRule.NumSeconds == null && limiterRule.NumPerTimespan == null)  // a rule with no limiter
                {
                    allowAccess = true;
                }
                else if (limiterRule.NumSeconds == null)  
                {
                    var numPerTimespan = limiterRule.NumPerTimespan ?? 0;
                    // the previous NumPerTimespan - 1 requests must all be unhandled before the limiter will allow the request.
                    var latestRequests = userRequests.OrderByDescending(x => x.CreatedDate).Take(numPerTimespan-1).Where(x => x.WasHandled == true).ToList();

                    if (latestRequests.Count() == 0)
                    {
                        allowAccess = true;  
                    }
                }
                else
                {
                    // implies the number of requests in 
                    var numSeconds = limiterRule.NumSeconds ?? 0;
                    var latestRequests = userRequests.OrderByDescending(x => x.CreatedDate).Where(x => x.CreatedDate.AddSeconds(numSeconds) > DateTime.Now);

                    if (latestRequests.Count() < limiterRule.NumPerTimespan)
                    {
                        allowAccess = true;
                    }

                }
            }

            // Record the request 
            //
            // Note that in production, the request auditing would probably be independent of
            // the limiter service and the limiter service would likely use that body
            // of data in its logic.  however, for this application, requests are only
            // recorded to support the limiter service so it lives here.

            request.WasHandled = allowAccess;
            await _requestsDataService.AddAsync(request);

            var jeff = await _requestsDataService.GetAllAsync();

            return allowAccess;
        }
    }
}
