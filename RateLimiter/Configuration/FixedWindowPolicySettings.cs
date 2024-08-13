namespace RateLimiter.Configuration;

public class FixedWindowPolicySettings : BasePolicySettings
{
    /// <summary>
    /// Defines how many requests during the specified time window is allowed by this policy
    /// </summary>
    public int MaxRequestsCount { get; set; } = 20;

    /// <summary>
    /// Defines fixed time window in seconds for the policy
    /// </summary>
    public int InSeconds { get; set; } = 1;
}
