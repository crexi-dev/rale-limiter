using System;
using System.Collections.Generic;

namespace RateLimiter.Models;

/// <summary>
/// Response for rate limit rule evaluations.
/// </summary>
public class RateLimitResponse 
{
    /// <summary>
    /// Indicates whether request passes rate limiting rule(s).
    /// </summary>
    public bool Allowed { get; }

    /// <summary>
    /// List of rule(s) that were rejected.
    /// </summary>
    public IReadOnlyList<string> RejectedReasons { get; } = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitResponse"/> with default values.
    /// </summary>
    public RateLimitResponse()
    {
        Allowed = true;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitResponse"/>. 
    /// </summary>
    /// <param name="allowed">True if the request is allowed; otherwise, false.</param>
    /// <param name="rejectedRules">List of rule(s) that request was rejected.</param>
    public RateLimitResponse(bool allowed, IReadOnlyList<string>? rejectedRules) 
    {
        Allowed = allowed;
        RejectedReasons = rejectedRules ?? Array.Empty<string>();
    }
}