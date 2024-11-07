using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace RateLimiter.CustomLimiters;

/// <summary>
/// Represents a lease for a random rate limiter.
/// </summary>
public class RandomRateLimiterLease : RateLimitLease
{
    private readonly bool _isAcquired;

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomRateLimiterLease"/> class.
    /// </summary>
    /// <param name="isAcquired">Indicates whether the lease was successfully acquired.</param>
    public RandomRateLimiterLease(bool isAcquired)
    {
        _isAcquired = isAcquired;
    }

    /// <summary>
    /// Gets a value indicating whether the lease was successfully acquired.
    /// </summary>
    public override bool IsAcquired => _isAcquired;

    /// <summary>
    /// Gets the names of the metadata associated with the lease.
    /// </summary>
    public override IEnumerable<string> MetadataNames => Array.Empty<string>();

    /// <summary>
    /// Tries to get the metadata associated with the specified name.
    /// </summary>
    /// <param name="metadataName">The name of the metadata.</param>
    /// <param name="metadata">The metadata value, if found.</param>
    /// <returns>True if the metadata is found; otherwise, false.</returns>
    public override bool TryGetMetadata(string metadataName, out object? metadata)
    {
        // No metadata is associated with this lease
        metadata = null;
        return false;
    }
}

