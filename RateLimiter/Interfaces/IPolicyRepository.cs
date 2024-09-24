using RateLimiter.Models.RatePolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for policy repository.
    /// </summary>
    public interface IPolicyRepository
    {
        Task<List<RatePolicy>?> GetResourceRatePoliciesForUser(string userId, string resourceId);
    }
}
