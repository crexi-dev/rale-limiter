using RateLimiter.Dtos;

namespace RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        bool IsRequestAllowed(RateLimitRuleRequestDto userInfo);
    }
}
