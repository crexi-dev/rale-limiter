using RateLimiter.Interfaces;
using RateLimiter.Models.RatePolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    /// <summary>
    /// Class for policy service.
    /// </summary>
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyVerifier _policyVerifier;

        public PolicyService(IPolicyRepository policyRepository, IPolicyVerifier policyVerifier)
        {
            _policyRepository = policyRepository;
            _policyVerifier = policyVerifier;
        }

        public IPolicyVerifier Verifier => _policyVerifier;

        public async Task<List<RatePolicy>?> GetRatePoliciesAsync(string userId, string resourceId)
        {
            // get resource access policies for this user
            return await _policyRepository.GetResourceRatePoliciesForUser(userId, resourceId);
        }
    }
}
