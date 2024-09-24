using Newtonsoft.Json;
using RateLimiter.Models.RatePolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RateLimiter.Helpers
{
    /// <summary>
    /// Factory to create all types of policies.
    /// </summary>
    public static class PolicyFactory
    {
        public static RatePolicy CreateRatePolicy(PolicyType policyType, string policyJson)
        {
            switch (policyType)
            {
                case PolicyType.NoLimit:
                    return new NolimitPolicy();

                case PolicyType.TimeSpan:
                    var timeSpanInfo = JsonConvert.DeserializeObject<TimeSpanInfo>(policyJson);
                    return new TimeSpanPolicy(policyType, timeSpanInfo);

                case PolicyType.LastCall:
                    var lastCallInfo = JsonConvert.DeserializeObject<LastCallInfo>(policyJson);
                    return new LastCallPolicy(policyType, lastCallInfo);
            }

            throw new NotImplementedException();
        }
    }
}
