using RateLimiter.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _repository;

        public ResourceService(IResourceRepository repository) 
        {
            _repository = repository;
        }

        public async Task<int> AddResource(string endpointUrl)
        {
            var Id = await _repository.AddResource(endpointUrl);
            return Id;
        }
    }
}
