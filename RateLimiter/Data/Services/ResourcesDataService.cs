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
        private readonly DbRepository<Resource> _resourcesRepository;

        public ResourcesDataService(DbRepository<Resource> resourcesRepository)
        {
            _resourcesRepository = resourcesRepository;
        }

        public async Task<List<Resource>> GetAllAsync()
        {
            string[] includes = new string[] { "" };

            var resources = await _resourcesRepository.GetAllAsync(includes);

            return resources;
        }
        public async Task<List<Resource>> FindAsync(BaseModel searchCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<Resource> SingleAsync(int id)
        {
            string[] includes = new string[] { "" };

            var resource = await _resourcesRepository.SingleAsync(id, includes);

            return resource;
        }
        public async Task<Resource?> SingleOrDefaultAsync(int id)
        {
            string[] includes = new string[] { "" };

            var resource = await _resourcesRepository.SingleOrDefaultAsync(id, includes);

            return resource;
        }
        public async Task<Resource> SingleAsync(string identifier)
        {
            string[] includes = new string[] { "" };

            var resource = await _resourcesRepository.SingleAsync(identifier, includes);

            return resource;
        }
        public async Task<bool> AddAsync(Resource resource)
        {
            var newResource = await _resourcesRepository.AddAsync(resource);

            return true;
        }
        public async Task<bool> UpdateAsync(int id, Resource resource)
        {
            string[] includes = new string[] { "" };

            var existingResource = await _resourcesRepository.SingleAsync(id, includes);

            existingResource.Name = resource.Name; 
            existingResource.Description = resource.Description;
            existingResource.StatusId = resource.StatusId;
            existingResource.UpdatedBy = resource.UpdatedBy;
            existingResource.UpdatedDate = DateTime.Now;

            await _resourcesRepository.UpdateAsync(existingResource);

            return true;
        }
        public async Task<bool> RemoveAsync(int id)
        {
            var newResource = await _resourcesRepository.RemoveAsync(id);

            return true;
        }
    }
}
