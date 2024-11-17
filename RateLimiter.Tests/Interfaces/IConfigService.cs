using RateLimiter.Data.Models.Data;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    // The config service is intended to add data to the database
    // for testing purposes.

    public interface IConfigService
    {
        public Task Reset();
        public Task SeedResources(List<Resource> resources);
        public Task SeedUsers(List<User> users);
        public Task SeedRequests(List<Request> requests);
        public Task SeedLimiterRules(List<LimiterRule> limiterRules);
    }
}
