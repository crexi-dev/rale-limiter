namespace Crexi.RateLimiter.Models;

/// <summary>
/// Primary output class from the rate limiter engine
/// </summary>
public class RateLimitEngineResult
{
    public required bool IsAllowed { get; set; }
    
    /// <summary>
    /// Indicates which policies failed for troubleshooting/logging purposes
    /// </summary>
    public List<string>? PassingPolicyNames{ get; set; }
    
    /// <summary>
    /// Indicates which policies failed for troubleshooting/logging purposes
    /// </summary>
    public List<string>? FailingPolicyNames{ get; set; }
}