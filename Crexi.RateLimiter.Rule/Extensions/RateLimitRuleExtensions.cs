using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Extensions;

public static class RateLimitRuleExtensions
{
    public static CallData ToCallData(this RateLimitRule rule) => new()
    {
        RegionId = rule.RegionId,
        TierId = rule.TierId,
        ClientId = rule.ClientId,
        Resource = rule.Resource,
    };
}