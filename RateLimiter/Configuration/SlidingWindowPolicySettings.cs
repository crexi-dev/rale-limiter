namespace RateLimiter.Configuration;

public class SlidingWindowPolicySettings : BasePolicySettings
{
    /// <summary>
    /// Defines minimal time interval between requests
    /// </summary>
    public int MinTimeoutInSeconds { get; set; } = 5;
}
