using RateLimiter.Base;
using RateLimiter.Config;
using System;
using System.Collections.Generic;

namespace RateLimiter;

/// <summary>
/// This class implements a linked rate limiter that combines multiple rate limiters.
/// It supports any combination of limiters, including multiple instances of the same limiter.
/// It does not currently support nested linked limiters, althought it could be enhanced to do so.
/// 
/// To acquire a lease, the linked limiter attempts to acquire a lease from each linked limiter.
/// All must succeed or the lease is not acquired.
/// 
/// If a lease is not acquired, any linked leases that were acquired are relinquished with any
/// acquired resources replentished.
/// </summary>
public class LinkedLimiter : BaseRateLimiter
{
    /// <summary>
    /// Gets or sets the list of linked limiters.
    /// </summary>
    private List<KeyValuePair<LimiterType, BaseRateLimiter>> LimiterList { get; set; } = new List<KeyValuePair<LimiterType, BaseRateLimiter>>();

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedLimiter"/> class.
    /// </summary>
    /// <param name="limiters">The list of rate limiters to be linked.</param>
    /// <param name="limiterConfig">The configuration for the linked limiter.</param>
    public LinkedLimiter(List<BaseRateLimiter> limiters, LimiterConfig limiterConfig) : base(limiterConfig)
    {
        foreach (var limiter in limiters)
        {
            LimiterList.Add(new KeyValuePair<LimiterType, BaseRateLimiter>(limiter.LimiterType, limiter));
        }
    }

    /// <summary>
    /// Acquires a lease based on the provided configuration.
    /// </summary>
    /// <param name="leaseConfig">The configuration for the lease to be acquired.</param>
    /// <returns>A lease object representing the acquired lease.</returns>
    public override BaseRateLimiterLease AcquireLease(LeaseConfig leaseConfig)
    {
        var linkedLeases = new List<BaseRateLimiterLease>();

        // Attempt to acquire a lease from each linked limiter using the shared leaseConfig.
        var areAllLeasesAcquired = true;
        foreach (var limiter in LimiterList)
        {
            var lease = limiter.Value.AcquireLease(leaseConfig);
            if (!lease.IsAcquired)
            {
                areAllLeasesAcquired = false;
            }
            linkedLeases.Add(lease);
        }

        // If not all leases were acquired, relinquish all leases to free up limiter availability.
        if (!areAllLeasesAcquired)
        {
            foreach (var lease in linkedLeases)
            {
                lease.RelinquishLease(true);
            }
        }

        var linkedLimiterLease = new LinkedLimiterLease(leaseConfig, areAllLeasesAcquired, linkedLeases, this);
        return linkedLimiterLease;
    }

    /// <summary>
    /// Releases the lease and optionally resets resources.
    /// </summary>
    /// <param name="lease">The lease to be released.</param>
    /// <param name="shouldResetResources">Indicates whether resources should be reset upon releasing the lease.</param>
    internal override void ReleaseLease(BaseRateLimiterLease lease, bool shouldResetResources = false)
    {
        if (!lease.IsRelinquished)
        {
            var linkedLease = lease as LinkedLimiterLease;
            if (linkedLease == null)
            {
                throw new InvalidOperationException("Incorrect lease type.");
            }
        }
    }
}