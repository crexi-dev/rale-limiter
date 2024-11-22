using RateLimiter.Config;
using System.Collections.Generic;

namespace RateLimiter.Base;

/// <summary>
/// Enum representing the different types of rate limiters available.
/// </summary>
public enum LimiterType
{
    TokenLimiter,
    FixedWindowLimiter,
    LinkedLimiter,
}

/// <summary>
/// Abstract base class for all rate limiters.
/// </summary>
public abstract class BaseRateLimiter
{
    /// <summary>
    /// The type of the rate limiter.
    /// </summary>
    public LimiterType LimiterType { get; }

    /// <summary>
    /// List of leases currently held by the rate limiter.
    /// </summary>
    protected List<BaseRateLimiterLease> Leases { get; set; } = new List<BaseRateLimiterLease>();

    /// <summary>
    /// Constructor to initialize the rate limiter with the given configuration.
    /// </summary>
    /// <param name="rateLimiterConfiguration">The configuration for the rate limiter.</param>
    public BaseRateLimiter(LimiterConfig rateLimiterConfiguration)
    {
        LimiterType = rateLimiterConfiguration.LimiterType;
    }

    /// <summary>
    /// Abstract method to acquire a lease based on the provided configuration.
    /// </summary>
    /// <param name="leaseConfig">The configuration for the lease to be acquired.</param>
    /// <returns>A lease object representing the acquired lease.</returns>
    public abstract BaseRateLimiterLease AcquireLease(LeaseConfig leaseConfig);

    /// <summary>
    /// This method should only be called from the RelinquishLease method of the lease and has been marked as internal.
    /// </summary>
    /// <param name="lease">The lease to be released.</param>
    /// <param name="shouldResetResources">Indicates whether resources should be reset upon releasing the lease.</param>
    internal abstract void ReleaseLease(BaseRateLimiterLease lease, bool shouldResetResources);
}

