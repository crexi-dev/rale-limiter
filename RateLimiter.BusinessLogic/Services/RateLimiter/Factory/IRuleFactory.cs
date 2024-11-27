using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.RateLimiter.Factory
{
	public interface IRuleFactory
	{
		IEnumerable<IRuleService> GetRulesByRegion(RegionType regionType);
		IRuleService GetRule(RuleType ruleType);
	}
}
