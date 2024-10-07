using RateLimiter.Interfaces;
using RateLimiter.Storage;

namespace RateLimiter.Results;
/// <summary>
/// Count based rules result object
/// </summary>
public class CountBasedRateLimiterResult : IRateLimiterResult
{
    /// <summary>
    /// Whether if the request was successful or not
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// Storage entry related to the request
    /// </summary>
    public CountBasedStorageEntry? StorageEntry { get; set; }
}
