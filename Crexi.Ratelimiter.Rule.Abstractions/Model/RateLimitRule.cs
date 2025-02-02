using Crexi.RateLimiter.Rule.Enum;

namespace Crexi.RateLimiter.Rule.Model;

public class RateLimitRule
{
    /// <summary>
    /// Limits the rule to a specific region
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Limits the rule to a tier of service
    /// </summary>
    public int? TierId { get; set; }

    /// <summary>
    /// Limits the rule to a specific client
    /// </summary>
    public int? ClientId { get; set; }

    /// <summary>
    /// The endpoint the rule applies to
    /// Sample expected format: `HTTP: GET /route/{id}`
    /// </summary>
    public required string Resource { get; set; }

    /// <summary>
    /// TimeSpan in milliseconds to use as the measurement
    /// </summary>
    public required long Timespan { get; set; }
    
    /// <summary>
    /// Max number of calls allowed
    /// Optional based on Evaluation Type.
    /// </summary>
    public int? MaxCallCount { get; set; }

    /// <summary>
    /// How the Rule timespan will be evaluated
    /// </summary>
    public required EvaluationType EvaluationType { get; set; }

    /// <summary>
    /// HttpResponseCode code to return
    /// If not provided, the default code will be used
    /// </summary>
    public int? OverrideResponseCode { get; set; }

    /// <summary>
    /// Defines optional window within which rule is active
    /// </summary>
    public TimeOnly? EffectiveWindowStartUtc { get; set; }

    /// <summary>
    /// Defines optional window within which rule is active
    /// </summary>
    public TimeOnly? EffectiveWindowEndUtc { get; set; }
}
