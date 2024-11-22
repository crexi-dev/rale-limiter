using RateLimiter.Base;
using System;
using System.Text.Json.Serialization;

namespace RateLimiter.Config;

/// <summary>
/// This class represents the configuration for a rate limiter. It combines all properties that 
/// can be used for each rate limiter. It is the responsibilty of the limiters to validate the
/// correct properties are set for the limiter type.
/// 
/// The class has been structured to support loading limiter configuration from JSON.
/// </summary>
public class LimiterConfig
{

    private LimiterType _limiterType;

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
    public LimiterType LimiterType
    {
        get => _limiterType;
        set => _limiterType = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of tokens that can be acquired in a single lease.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Gets or sets the maximum time in seconds that the limiter will be active.
    /// </summary>
    public int? MaxTimeInSeconds { get; set; }

}
