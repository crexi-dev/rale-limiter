
using System;
using System.Collections.Generic;

namespace RateLimiter.Models
{
    public class RateLimitingRuleConfiguration
    {
        public string Controller { get; set; } = default!;
        public string Action { get; set; } = default!;
        public TimeSpan Timerange { get; set; }
        public int NumberOfRequests { get; set; }
        public string Country { get; set; } = default!;
        public List<string>? Parameters { get; set; } = default!;
    }
}
