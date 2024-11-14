using M42.Data.Repositories;
using RateLimiter.Data.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class ResourcesDataService : IDataService<Resource>
    {
        private readonly DbRepository<Resource> _resourceRepository;

        public ResourcesDataService(DbRepository<Resource> resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<List<Resource>> Get()
        {
            throw new NotImplementedException();
        }
        public async Task<List<Resource>> Get(BaseModel searchCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<Resource> Get(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<Resource> Get(string identifier)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Add(Resource resource)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(int id, Resource resource)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
