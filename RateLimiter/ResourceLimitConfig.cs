using System.Collections.Generic;

namespace RateLimiter;

/// <summary>
/// Configuration class for defining the resource limits and their associated rate limiters.
/// </summary>
public class ResourceLimitConfig
{
    /// <summary>
    /// Gets or sets the name of the resource.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of rate limiter configurations for the resource.
    /// </summary>
    public List<RateLimiterConfig> Limiters { get; set; } = new();
}
