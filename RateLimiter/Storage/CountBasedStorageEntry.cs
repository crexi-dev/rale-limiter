using RateLimiter.Interfaces;
using System;

namespace RateLimiter.Storage;

/// <summary>
/// Storage entry used to analyze requests
/// </summary>
public class CountBasedStorageEntry : IRateLimiterStorageEntry
{
    /// <summary>
    /// Amount of accesses
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Last access Time
    /// </summary>
    public DateTime LastAccessTime { get; set; }
}
