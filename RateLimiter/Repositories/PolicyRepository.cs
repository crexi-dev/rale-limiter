using RateLimiter.Helpers;
using RateLimiter.Interfaces;
using RateLimiter.Models.RatePolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    /// <summary>
    /// Class for policy repository.
    /// </summary>
    public class PolicyRepository : IPolicyRepository
    {
        private readonly IPersistentProvider _persistentProvider;
        public PolicyRepository(IPersistentProvider persistentProvider)
        {
            _persistentProvider = persistentProvider;
        }

        public async Task<List<RatePolicy>?> GetResourceRatePoliciesForUser(string userId, string resourceId)
        {
            var result = new List<RatePolicy>();

            var policies = await _persistentProvider.GetUserResourcePoliciesAsync(userId, resourceId);
            if (policies?.Any() == true)
            {
                foreach (var policy in policies)
                {
                    if (Enum.TryParse(policy.PolicyName, out PolicyType policyType))
                    {
                        result.Add(PolicyFactory.CreateRatePolicy(policyType, policy.PolicyJson));
                    }
                }
            }

            return result;
        }
    }
}
