using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for all persistent operations.
    /// </summary>
    public interface IPersistentProvider : IPolicyVerifier
    {
        Task<User?> GetUserAsync(string userId);
        Task<Policy?> GetPolicyAsync(string policyId);
        Task<Resource?> GetResourceAsync(string resourceId);
        Task<List<Policy>?> GetUserResourcePoliciesAsync(string userId, string resourceId);
        Task AddUserAccessAsync(string userId, string resourceId, DateTime accessTime);
    }
}
