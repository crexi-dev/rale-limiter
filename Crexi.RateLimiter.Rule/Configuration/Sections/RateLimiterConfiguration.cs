namespace Crexi.RateLimiter.Rule.Configuration.Sections;

public sealed class RateLimiterConfiguration
{
    public long MaxTimeSpanMinutes { get; set; }
    public bool UnrecognizedEvaluationTypeEvaluationResult { get; set; }
    public int? UnrecognizedEvaluationTypeOverrideResponseCode { get; set; }
}