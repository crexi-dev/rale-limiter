using RateLimiter.Dtos;

namespace RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo);
    }
}
