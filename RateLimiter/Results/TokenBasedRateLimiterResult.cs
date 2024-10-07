using RateLimiter.Interfaces;
using RateLimiter.Storage;

namespace RateLimiter.Results;

/// <summary>
/// Token based rule results
/// </summary>
public class TokenBasedRateLimiterResult : IRateLimiterResult
{
    public bool Success { get; set; }

    /// <summary>
    /// Storage entry
    /// </summary>
    public TokenBasedStorageEntry? StorageEntry { get; set; }
}
