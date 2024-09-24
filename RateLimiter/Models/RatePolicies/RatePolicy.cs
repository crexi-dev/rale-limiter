using RateLimiter.Interfaces;
using RateLimiter.Models.Apis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.RatePolicies
{
    /// <summary>
    /// Base class for policy.
    /// </summary>
    public abstract class RatePolicy
    {
        public abstract Task<PolicyStatus> GetPolicyStatus(IPolicyVerifier policyVerifier, RateLimitRequest request);
    }
}
