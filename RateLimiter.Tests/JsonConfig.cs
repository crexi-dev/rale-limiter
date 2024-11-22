using System.Collections.Generic;

namespace RateLimiter.Config;

/// <summary>
/// Used to deserialize the JSON configuration file to typed objects.
/// </summary>
public class JsonConfig
{
    public List<ResourceLimiterConfig> RateLimiterConfig { get; set; }
}

/// <summary>
/// Used to deserialize the JSON configuration file to typed objects.
/// </summary>
public class ResourceLimiterConfig
{
    public List<LeaseConfig> Resources { get; set; }

    public List<LimiterConfig> Limiters { get; set; }
}