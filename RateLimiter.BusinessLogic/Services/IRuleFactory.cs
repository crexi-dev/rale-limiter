using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services
{
	public interface IRuleFactory
	{
		IRuleService GetRule(RegionType regionType, RuleType ruleType);
	}
}
