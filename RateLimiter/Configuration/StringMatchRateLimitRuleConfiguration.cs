
namespace RateLimiter.Configuration
{
    /// <summary>
    /// Configuration to indicate the behavior of a simple StringMatchRateLimiter
    /// </summary>
    public sealed class StringMatchRateLimitRuleConfiguration
    {
        /// <summary>
        /// Indicates the string to match for the requests. Must not be empty or null
        /// </summary>
        public string Match { get; set; } = string.Empty;
    }
}

