using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for resource repository.
    /// </summary>
    public interface IResourceRepository
    {
        Task AddResourceAsync(Resource resource);
        Task RemoveResourceAsync(string resourceId);
        Task<Resource> GetResourceAsync(string resourceId);
    }
}
