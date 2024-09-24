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
    /// Class for no limit policy.
    /// </summary>
    public class NolimitPolicy : RatePolicy
    {
        public override async Task<PolicyStatus> GetPolicyStatus(IPolicyVerifier policyVerifier, RateLimitRequest request)
        {
            return new PolicyStatus()
            {
                IsConforming = true,
            };
        }
    }
}
