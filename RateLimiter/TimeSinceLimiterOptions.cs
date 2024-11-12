using System;

namespace RateLimiter;

/// <summary>
/// Options class for configuring the TimeSinceLimiter.
/// </summary>
/// <remarks>
/// The <see cref="AllowedPeriod"/> defines the time span that must have elapsed
/// between successive permitted operations to avoid triggering the limiter.
/// </remarks>
public class TimeSinceLimiterOptions
{
    /// <summary>
    /// Defines the time span that must have elapsed between successive permitted operations
    /// </summary>
    public required TimeSpan AllowedPeriod { get; set; }
}