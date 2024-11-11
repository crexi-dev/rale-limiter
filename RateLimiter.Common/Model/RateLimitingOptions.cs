using RateLimiter.Common.Enum;

namespace RateLimiter.Common.Model;

public record RateLimitingOptions
{
    /// <summary>
    /// The configured method / rule. An enum flag that can contain multiple rules
    /// </summary>
    public RateLimitingMethod Method { get; set; } = RateLimitingMethod.RequestsPerTimespan;

    /// <summary>
    /// The maximum number of requests allowed with the specific method.
    /// </summary>      
    public int MaxRequests { get; set; } = 1;

    /// <summary>
    /// Configured Timespan for this RateLimiting Method
    /// </summary>
    public TimeSpan TimeSpan { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The set of origin country codes that this rate limiting method is configured to apply to
    /// </summary>
    public HashSet<string>? ApplicableCountryCodes;
}
