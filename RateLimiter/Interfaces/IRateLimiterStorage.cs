using System;

namespace RateLimiter.Interfaces;

/// <summary>
/// Interface for Rate Limiter Storage
/// The class that implements this interface must be thread safe
/// </summary>
public interface IRateLimiterStorage
{
    /// <summary>
    /// Sets a value in the storage using a given key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IRateLimiterStorageEntry Set(string key, IRateLimiterStorageEntry value);
    /// <summary>
    /// Sets a value in the storage using a given key and extra parameters
    /// </summary>
    /// <returns></returns>
    IRateLimiterStorageEntry Set(string key, TimeSpan expiry, IRateLimiterStorageEntry value);
    /// <summary>
    /// Gets or creates an entry in the storage using a given key
    /// </summary>
    /// <returns></returns>
    IRateLimiterStorageEntry GetOrCreate(string key, TimeSpan expiry, IRateLimiterStorageEntry defaultValue);
}
