using RateLimiter.Interfaces;
using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    /// <summary>
    /// Class for resource repository.
    /// </summary>
    public class ResourceRepository : IResourceRepository
    {
        private readonly IPersistentProvider _persistentProvider;
        public ResourceRepository(IPersistentProvider persistentProvider)
        {
            _persistentProvider = persistentProvider;
        }

        public Task AddResourceAsync(Resource resource)
        {
            throw new NotImplementedException();
        }

        public Task RemoveResourceAsync(string resourceId)
        {
            throw new NotImplementedException();
        }

        public async Task<Resource> GetResourceAsync(string resourceId)
        {
            return await _persistentProvider.GetResourceAsync(resourceId);
        }

    }
}
