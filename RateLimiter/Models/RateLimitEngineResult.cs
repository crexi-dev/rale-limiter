namespace Crexi.RateLimiter.Models;

public class RateLimitEngineResult
{
    public required bool IsAllowed { get; set; }
    public List<string>? PassingPolicyNames{ get; set; }
    public List<string>? FailingPolicyNames{ get; set; }
}