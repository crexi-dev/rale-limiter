using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Contracts;

public interface IRateLimitRule
{
    public RegionType RegionType { get; }
    
    public RuleType RuleType { get; }
    
    bool Validate(Request request);
}