using RateLimiter.Base;

namespace RateLimiter;

/// <summary>
/// This class represents a lease for the TokenLimiter.
/// Tokens can be configured to weight the requests by resource.
/// </summary>
public class TokenLimiterLease : BaseRateLimiterLease
{
    /// <summary>
    /// Gets the number of tokens associated with this lease.
    /// </summary>
    public int Tokens { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenLimiterLease"/> class.
    /// </summary>
    /// <param name="resourceName">The name of the resource associated with this lease.</param>
    /// <param name="limiter">The token limiter associated with this lease.</param>
    /// <param name="isAcquired">Indicates whether the lease was successfully acquired.</param>
    /// <param name="tokens">The number of tokens associated with this lease.</param>
    public TokenLimiterLease(string resourceName, TokenLimiter limiter, bool isAcquired, int tokens)
        : base(resourceName, isAcquired, limiter)
    {
        Tokens = tokens;
    }
}

