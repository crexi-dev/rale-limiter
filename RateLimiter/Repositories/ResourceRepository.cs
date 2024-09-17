using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        public ApplicationDBContext _dbContext { get; set; }
        public ResourceRepository(ApplicationDBContext dbContext) { 
            _dbContext = dbContext;
        }

        public async Task<int> AddResource(string endpointUrl)
        {
            try
            {
                Resource resource = new Resource()
                {
                    EndpointUrl = endpointUrl
                };
                await _dbContext.Resources.AddAsync(resource);
                await _dbContext.SaveChangesAsync();
                return resource.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetResourceId(string endPoint)
        {
            return _dbContext.Resources.Where(x => x.EndpointUrl.ToLower().Equals(endPoint))
                                             .Select(x => x.Id).FirstOrDefault();
        }
    }
}
