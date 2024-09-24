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
    /// Class for last call policy info.
    /// </summary>
    public class LastCallInfo
    {
        public int MinLastCallSeconds { get; set; }
    }

    /// <summary>
    /// Class for last call policy.
    /// </summary>
    public class LastCallPolicy : RatePolicy
    {
        private readonly PolicyType _policyType;
        private readonly LastCallInfo _lastCallInfo;

        public LastCallPolicy(PolicyType policyType, LastCallInfo lastCallInfo)
        {
            _policyType = policyType;
            _lastCallInfo = lastCallInfo;
        }

        public override async Task<PolicyStatus> GetPolicyStatus(IPolicyVerifier policyVerifier, RateLimitRequest request)
        {
            var requestsInTimeSpan = await policyVerifier.GetAccessCountInSecondsAsync(request.UserId, request.ResourceId, _lastCallInfo.MinLastCallSeconds);
            if (requestsInTimeSpan > 0)
            {
                return new PolicyStatus()
                {
                    IsConforming = false,
                    NotConformingReason = $"Only one call can be made in the last {_lastCallInfo.MinLastCallSeconds} seconds."
                };
            }

            return new PolicyStatus()
            {
                IsConforming = true,
            };
        }
    }
}
