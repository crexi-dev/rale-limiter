using Newtonsoft.Json;
using RateLimiter.Enums;
using System;


namespace RateLimiter
{
    /// <summary>
    /// Rules in RateLimitRules.json
    /// </summary>
    public class RateLimiterRule
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Active")]
        public bool Active { get; set; }

        [JsonProperty("Type")]
        public RuleType Type { get; set; }

        [JsonProperty("RuleStrategy")]
        public RuleStrategy RuleStrategy { get; set; }

        [JsonProperty("MaxRequests")]
        public int? MaxRequests { get; set; }

        [JsonProperty("TimespanSec")]
        public int? TimespanSec { get; set; }

        [JsonProperty("MininumIntervalSec")]
        public int? MininumIntervalSec { get; set; }

        [JsonProperty("Region")]
        public string Region { get; set; }
    }
}
