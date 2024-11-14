using RateLimiter.Data.Interfaces;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IDataService<Resource> _resourceDataService;
        private readonly IDataService<User> _userDataService;
        private readonly IDataService<Status> _statusesDataService;

        public ConfigService(IDataService<Resource> resourceDataService, IDataService<User> userDataService, IDataService<Status> statusesDataService)
        {
            _resourceDataService = resourceDataService;
            _userDataService = userDataService;
            _statusesDataService = statusesDataService;
        }

        public async Task<bool> SeedStatuses(List<Status> statuses)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> SeedResources(List<Resource> resources)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> SeedUsers(List<User> users)
        {
            throw new NotImplementedException();
        }
    }
}
