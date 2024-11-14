using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    public interface IConfigService
    {
        public Task<bool> SeedStatuses(List<Status> statuses);
        public Task<bool> SeedResources(List<Resource> resources);
        public Task<bool> SeedUsers(List<User> users);
    }
}
