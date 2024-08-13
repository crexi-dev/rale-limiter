using RateLimiter.Interface;

namespace RateLimiter.Interface
{
    public interface IRateLimiterRegionRuleService
    {
        IEnumerable<IRateLimiterRule> GetRulesByRegion(string region);
    }
}
