using RateLimiter.Base;
using RateLimiter.Config;
using System.Collections.Generic;

namespace RateLimiter;

/// <summary>
/// This class represents a lease that is linked to multiple rate limiters
/// via the linked limiter. When the lease is relinquished, all linked leases
/// are also released.
/// </summary>
public class LinkedLimiterLease : BaseRateLimiterLease
{
    /// <summary>
    /// Gets or sets the list of linked leases.
    /// </summary>
    private List<BaseRateLimiterLease> LinkedLeases { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedLimiterLease"/> class.
    /// </summary>
    /// <param name="leaseConfig">The configuration for the lease.</param>
    /// <param name="isAcquired">Indicates whether the lease was successfully acquired.</param>
    /// <param name="linkedLeases">The list of linked leases.</param>
    /// <param name="limiter">The linked limiter associated with this lease.</param>
    public LinkedLimiterLease(LeaseConfig leaseConfig, bool isAcquired, List<BaseRateLimiterLease> linkedLeases, LinkedLimiter limiter)
        : base(leaseConfig.ResourceName, isAcquired, limiter)
    {
        LinkedLeases = linkedLeases;

        // When not all leases were acquired, dispose any active leases to free up limiter availability.
        if (!isAcquired)
        {
            DisposeActiveLeases();
        }
    }

    /// <summary>
    /// Adds a linked lease to the list of linked leases.
    /// </summary>
    /// <param name="lease">The lease to be added.</param>
    public void AddLinkedLease(BaseRateLimiterLease lease)
    {
        LinkedLeases.Add(lease);
    }

    /// <summary>
    /// Disposes any active leases to free up limiter availability.
    /// </summary>
    protected void DisposeActiveLeases()
    {
        foreach (var lease in LinkedLeases)
        {
            if (lease.IsAcquired)
                lease.RelinquishLease();
        }
    }

    /// <summary>
    /// Relinquishes the lease and optionally resets resources.
    /// </summary>
    /// <param name="shouldResetResources">Indicates whether resources should be reset upon relinquishing the lease.</param>
    public override void RelinquishLease(bool shouldResetResources)
    {
        if (!IsRelinquished)
        {
            base.RelinquishLease();
            foreach (var lease in LinkedLeases)
            {
                lease.RelinquishLease(shouldResetResources);
            }
        }
    }
}

