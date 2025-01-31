namespace RateLimiter.Models;

public class RuleResult
{
    public bool IsAllowed { get; set; }
    public string RuleMessage { get; set; }
}
