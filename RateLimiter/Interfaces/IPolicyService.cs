using RateLimiter.Models.RatePolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for policy service.
    /// </summary>
    public interface IPolicyService
    {
        IPolicyVerifier Verifier { get; }
        Task<List<RatePolicy>?> GetRatePoliciesAsync(string userId, string resourceId);
    }
}
