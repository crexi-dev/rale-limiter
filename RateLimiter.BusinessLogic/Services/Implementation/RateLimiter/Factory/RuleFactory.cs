using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Factory
{
	public class RuleFactory : IRuleFactory
	{
		private readonly IEnumerable<IRuleService> _rules;

		public RuleFactory(IEnumerable<IRuleService> rules)
		{
			_rules = rules;
		}

		public IRuleService GetRule(RegionType regionType, RuleType ruleType)
			=> _rules.Single(x => x.RegionType == regionType && x.RuleType == ruleType);
	}
}
