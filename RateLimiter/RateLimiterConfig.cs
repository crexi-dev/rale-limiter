using System.Text.Json.Serialization;
using System;

namespace RateLimiter;

/// <summary>
/// Configuration class for defining the properties of a rate limiter.
/// </summary>
public class RateLimiterConfig
{

    private ResourceRateLimiter.RateLimiterType _limiterType;

    /// <summary>
    /// Gets or sets the type of the rate limiter as a string.
    /// </summary>
    [JsonPropertyName("LimiterType")]
    public string LimiterTypeString
    {
        get => _limiterType.ToString();
        set
        {
            if (!Enum.TryParse(value, out _limiterType))
            {
                throw new ArgumentException($"Invalid limiter type: {value}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the type of the rate limiter.
    /// </summary>
    [JsonIgnore]
    public ResourceRateLimiter.RateLimiterType LimiterType
    {
        get => _limiterType;
        set => _limiterType = value;
    }

    /// <summary>
    /// Gets or sets the minimum number of permits for the rate limiter.
    /// </summary>
    public int MinPermits { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of permits for the rate limiter.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Gets or sets the queue limit for the rate limiter.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the window duration in minutes for the rate limiter.
    /// </summary>
    public int WindowMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens per period for the token bucket rate limiter.
    /// </summary>
    public int TokensPerPeriod { get; set; }

    /// <summary>
    /// Gets or sets the replenishment period in seconds for the token bucket rate limiter.
    /// </summary>
    public int ReplenishmentPeriodSeconds { get; set; }

    /// <summary>
    /// Gets or sets the number of segments per window for the sliding window rate limiter.
    /// </summary>
    public int SlidingWindowSegments { get; set; }
}
