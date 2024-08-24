using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RateLimiter
{
    /// <summary>
    /// For RateLimitRules.json 
    /// </summary>
    public class RateLimiterConfig
    {
        [JsonProperty("RateLimiterRules")]
        public List<RateLimiterRule> Rules { get; set; }
    }
}
