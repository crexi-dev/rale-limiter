using System;

namespace RateLimiter.Base;

/// <summary>
/// This class allows the client to acquire a lease from the limiter.
/// It should be used as the lease for a limiter which does not require any additional properties.
/// </summary>
public class BaseRateLimiterLease
{
    /// <summary>
    /// The rate limiter associated with this lease.
    /// </summary>
    protected BaseRateLimiter Limiter { get; }

    /// <summary>
    /// Represents whether or not this lease has been relinquished by the client.
    /// </summary>
    public bool IsRelinquished { get; protected set; } = false;

    /// <summary>
    /// Represents whether or not the lease was successfully acquired by the limiter.
    /// </summary>
    public virtual bool IsAcquired { get; protected set; }

    /// <summary>
    /// The name of the resource associated with this lease.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// The unique identifier for this lease.
    /// </summary>
    public string LeaseId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRateLimiterLease"/> class.
    /// </summary>
    /// <param name="resourceName">The name of the resource associated with this lease.</param>
    /// <param name="isAcquired">Indicates whether the lease was successfully acquired.</param>
    /// <param name="limiter">The rate limiter associated with this lease.</param>
    public BaseRateLimiterLease(string resourceName, bool isAcquired, BaseRateLimiter limiter)
    {
        ResourceName = resourceName;
        LeaseId = Guid.NewGuid().ToString();
        Limiter = limiter;
        IsAcquired = isAcquired;
    }

    /// <summary>
    /// Relinquishes the lease, optionally resetting resources.
    /// </summary>
    /// <param name="shouldResetResources">Indicates whether resources should be reset upon relinquishing the lease.</param>
    public virtual void RelinquishLease(bool shouldResetResources = false)
    {
        if (!IsRelinquished)
        {
            Limiter.ReleaseLease(this, shouldResetResources);
            IsAcquired = false;
            IsRelinquished = true;
        }
    }
}
