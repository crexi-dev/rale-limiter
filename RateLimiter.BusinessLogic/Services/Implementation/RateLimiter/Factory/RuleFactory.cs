using RateLimiter.BusinessLogic.Services.RateLimiter.Factory;
using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
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

		public IEnumerable<IRuleService> GetRulesByRegion(RegionType regionType)
			=> _rules.Where(x => x.RegionType == regionType);

		public IRuleService GetRule(RuleType ruleType)
			=> _rules.Single(x => x.RuleType == ruleType); 
	}
}
