using System;

namespace RateLimiter.Configuration
{
    /// <summary>
    /// Configuration to indicate the behavior of a FixedWindowRateLimiter
    /// </summary>
    public sealed class FixedWindowRateLimitRuleConfiguration
    {
        /// <summary>
        /// Indicates the time window for the requests and must be set to a value greater than zero
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Indicates the upper limit for the number of requests that can be allowed in the above time window
        /// and must be set to a value greater than zero
        /// </summary>
        public int Limit { get; set; }
    }
}

