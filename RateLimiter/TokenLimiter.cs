using RateLimiter.Base;
using RateLimiter.Config;
using System;
using System.Linq;

namespace RateLimiter;

/// <summary>
/// This class implements a token-based rate limiter.
/// This limiter supports resources acquiring varying amounts of tokens via the lease configuration.
/// </summary>
public class TokenLimiter : BaseRateLimiter
{
    /// <summary>
    /// Gets the maximum number of tokens allowed.
    /// </summary>
    public int MaxTokens { get; }

    /// <summary>
    /// Gets or sets the number of available tokens.
    /// </summary>
    public int AvailableTokens { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenLimiter"/> class.
    /// </summary>
    /// <param name="limiterConfig">The configuration for the rate limiter.</param>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is invalid.</exception>
    public TokenLimiter(LimiterConfig limiterConfig) : base(limiterConfig)
    {
        if (limiterConfig == null || limiterConfig.MaxTokens == null || limiterConfig.MaxTokens < 1)
        {
            throw new ArgumentNullException($"{nameof(limiterConfig)}: Invalid configuration.");
        }

        MaxTokens = limiterConfig.MaxTokens.Value;
        AvailableTokens = MaxTokens;
    }

    /// <summary>
    /// Acquires a lease based on the provided configuration.
    /// </summary>
    /// <param name="leaseConfig">The configuration for the lease to be acquired.</param>
    /// <returns>A lease object representing the acquired lease.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the lease configuration is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when the number of tokens requested is invalid.</exception>
    public override TokenLimiterLease AcquireLease(LeaseConfig leaseConfig)
    {
        if (leaseConfig == null)
        {
            throw new ArgumentNullException($"{nameof(leaseConfig)}: Invalid lease configuration.");
        }

        if (leaseConfig.Tokens == null)
        {
            throw new ArgumentNullException($"{nameof(leaseConfig.Tokens)}: Invalid number of tokens requested.");
        }

        if (leaseConfig.Tokens < 1)
        {
            throw new ArgumentException($"{nameof(leaseConfig.Tokens)}: Invalid number of tokens requested.");
        }

        var lease = AvailableTokens < leaseConfig.Tokens ?
            new TokenLimiterLease(leaseConfig.ResourceName, this, false, 0) :
            new TokenLimiterLease(leaseConfig.ResourceName, this, true, leaseConfig.Tokens.Value);

        if (lease.IsAcquired)
        {
            AvailableTokens -= leaseConfig.Tokens.Value;
        }

        Leases.Add(lease);
        return lease;
    }

    /// <summary>
    /// Releases the lease and restores the tokens.
    /// </summary>
    /// <param name="lease">The lease to be released.</param>
    /// <param name="shouldResetResources">Not used in this method, resources are always restored in 
    /// the token limiter when a lease is released.</param>
    /// <exception cref="ArgumentNullException">Thrown when the lease is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when the lease does not belong to this limiter.</exception>
    /// <exception cref="Exception">Thrown when the available tokens exceed the maximum tokens allowed.</exception>
    internal override void ReleaseLease(BaseRateLimiterLease lease, bool shouldResetResources = false)
    {
        if (!lease.IsRelinquished)
        {
            var tokenLimiterLease = lease as TokenLimiterLease;
            if (tokenLimiterLease == null)
            {
                throw new ArgumentNullException($"{nameof(lease)}: Invalid lease.");
            }

            var leaseToRemove = Leases.FirstOrDefault(d => d.LeaseId == lease.LeaseId);
            if (leaseToRemove == null)
            {
                throw new ArgumentException("This lease does not belong to this limiter.");
            }

            if (lease.IsAcquired)
            {
                if (AvailableTokens + tokenLimiterLease.Tokens > MaxTokens)
                {
                    throw new Exception("Invalid state reached: available tokens exceeds the maximum tokens allowed for this limiter.");
                }

                AvailableTokens += tokenLimiterLease.Tokens;
            }

            Leases.Remove(leaseToRemove);
        }
    }
}

