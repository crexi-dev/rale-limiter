using RateLimiter.Base;
using RateLimiter.Config;
using System;
using System.Linq;

namespace RateLimiter;

/// <summary>
/// This class implements a fixed window rate limiter.
/// It configures the window's timeframe in seconds
/// and the maximum number of tokens allowed in the window.
/// </summary>
public class FixedWindowLimiter : BaseRateLimiter
{
    /// <summary>
    /// Gets the last token refill time in UTC.
    /// </summary>
    public DateTime LastRefillTimeUtc { get; private set; }

    /// <summary>
    /// Gets the time window in seconds.
    /// </summary>
    public int TimeInSeconds { get; }

    /// <summary>
    /// Gets the maximum number of tokens allowed in the time window.
    /// </summary>
    public int MaxTokens { get; }

    /// <summary>
    /// Gets the current number of tokens used in the time window.
    /// </summary>
    public int CurrentTokens { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedWindowLimiter"/> class.
    /// </summary>
    /// <param name="config">The configuration for the rate limiter.</param>
    public FixedWindowLimiter(LimiterConfig config)
        : base(config)
    {
        if (config.LimiterType != LimiterType.FixedWindowLimiter)
        {
            throw new ArgumentException("Invalid limiter type");
        }
        if (config.MaxTimeInSeconds == null)
        {
            throw new ArgumentException("MaxTimeInSeconds is required for FixedWindowLimiter");
        }
        if (config.MaxTokens == null)
        {
            throw new ArgumentException("MaxTokens is required for FixedWindowLimiter");
        }

        TimeInSeconds = config.MaxTimeInSeconds.Value;
        MaxTokens = config.MaxTokens.Value;
        LastRefillTimeUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Acquires a lease based on the provided configuration.
    /// </summary>
    /// <param name="leaseConfig">The configuration for the lease to be acquired.</param>
    /// <returns>A lease object representing the acquired lease.</returns>
    public override BaseRateLimiterLease AcquireLease(LeaseConfig leaseConfig)
    {
        // Always check window elapsed time to see if we need to reset the tokens.
        if (DateTime.UtcNow > LastRefillTimeUtc.AddSeconds(TimeInSeconds))
        {
            LastRefillTimeUtc = DateTime.UtcNow;
            CurrentTokens = 0;
        }

        // We reuse BaseRateLimiterLease here, but we could create an inherited class to:
        // 1. Add a property to store the number of tokens used if we enhanced the fixed window algorithm.
        // 2. Add a property to store some of the metrics from the Limiter (timespan, elapsed time, etc).
        var lease = CurrentTokens < MaxTokens
            ? new BaseRateLimiterLease(leaseConfig.ResourceName, true, this)
            : new BaseRateLimiterLease(leaseConfig.ResourceName, false, this);

        if (CurrentTokens < MaxTokens)
        {
            CurrentTokens++;
        }

        Leases.Add(lease);
        return lease;
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
            if (lease == null)
            {
                throw new ArgumentNullException($"{nameof(lease)}: Invalid lease.");
            }

            var leaseToRemove = Leases.FirstOrDefault(d => d.LeaseId == lease.LeaseId);
            if (leaseToRemove == null)
            {
                throw new ArgumentException("This lease does not belong to this limiter.");
            }

            // Primarily used by chained limiters to reset resources when a partial success occurs.
            if (shouldResetResources)
            {
                CurrentTokens--;
            }

            Leases.Remove(leaseToRemove);
        }
    }
}

