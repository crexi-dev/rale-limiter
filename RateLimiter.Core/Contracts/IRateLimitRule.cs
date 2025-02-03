using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Contracts;

public interface IRateLimitRule
{
    public RuleType RuleType { get; }
    
    bool Validate(Request request);
}