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
    /// Class for time span policy info.
    /// </summary>
    public class TimeSpanInfo
    {
        public int SpanInSeconds { get; set; }
        public int MaxCalls { get; set; }
    }

    /// <summary>
    /// Class for time span policy.
    /// </summary>
    public class TimeSpanPolicy : RatePolicy
    {
        private readonly PolicyType _policyType;
        private readonly TimeSpanInfo _timeSpanInfo;

        public TimeSpanPolicy(PolicyType policyType, TimeSpanInfo timeSpanInfo)
        {
            _policyType = policyType;
            _timeSpanInfo = timeSpanInfo;
        }

        public override async Task<PolicyStatus> GetPolicyStatus(IPolicyVerifier policyVerifier, RateLimitRequest request)
        {
            var requestsInTimeSpan = await policyVerifier.GetAccessCountInSecondsAsync(request.UserId, request.ResourceId, _timeSpanInfo.SpanInSeconds);
            if (requestsInTimeSpan < _timeSpanInfo.MaxCalls)
            {
                return new PolicyStatus()
                {
                    IsConforming = true,
                };
            }

            return new PolicyStatus()
            {
                IsConforming = false,
                NotConformingReason = $"Too many requests in the last {_timeSpanInfo.SpanInSeconds} seconds."
            };
        }
    }
}
