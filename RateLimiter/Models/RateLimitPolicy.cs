namespace Crexi.RateLimiter.Models;

/// <summary>
/// RateLimitPolicy stores the configuration of a single rate limiting rule   
/// </summary>
public class RateLimitPolicy
{
    /// <summary>
    /// PolicyName should be unique with convention ApiName-PolicyName-Filters, ie ListingsApi-SlidingWindow-US-CA
    /// Primarily for logging which policy was triggered when requests exceed the threshold
    /// </summary>
    public required string PolicyName { get; init; }
    
    /// <summary>
    /// PolicyType enum specifies what type of rate limit algorithm is applied (similar to .NET RateLimiting naming)
    /// </summary>
    public required PolicyType PolicyType  { get; init; }
    
    /// <summary>
    /// Limit number denotes the inclusive threshold number of requests allowed
    /// </summary>
    public required long Limit { get; set; }
    
    /// <summary>
    /// Used to create a timespan for fixed and sliding window rate limits
    /// </summary>
    public TimeSpan? TimeSpanWindow { get; set; }

    /// <summary>
    /// When ApplyClientTagFilter is true, only requests that have the specified tags will be evaluated for this policy
    /// </summary>
    public bool ApplyClientTagFilter { get; set; } = false;

    /// <summary>
    /// ClientFilters verify if the client request should fall under this policy.
    /// Tags defined here will be matched up to the client request.
    /// Examples: Region=["CA-US","ON-CA", "JP-26"], SubscriptionTier=["FREE"]
    /// </summary>
    public List<ClientFilterGroup> ClientFilterGroups { get; set; } = new();
}


