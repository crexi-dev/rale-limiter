namespace Crexi.RateLimiter.Models;

/// <summary>
/// Multiple client filters can have an rate limit override
/// Example: California users with a premium subscription can have a have a higher rate limit 
/// </summary>
public class ClientFilterGroup
{
    public required List<ClientFilter> ClientFilters { get; set; } = new();
    
    /// <summary>
    /// Threshold number that either increases or decreases the default request limit
    /// </summary>
    public required long LimitOverride { get; set; }
}