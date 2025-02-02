using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Configuration.Sections;

public class RateLimitRulesConfiguration
{
    public IEnumerable<RateLimitRule>? StartupRules { get; set; }
}