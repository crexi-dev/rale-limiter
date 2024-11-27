using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.RateLimiter;
using RateLimiter.BusinessLogic.Services.RateLimiter.Factory;
using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.Implementation.RateLimiter
{
	public class LimitService : ILimitService
	{
		private readonly IRuleFactory _ruleFactory;

		public LimitService(IRuleFactory ruleFactory)
		{
			_ruleFactory = ruleFactory;
		}

		public async Task<bool> IsRequestReachedLimit(List<RuleType> appliedRuleTypes, RequestDto requestModel)
		{
			var rulesByRegion = _ruleFactory.GetRulesByRegion(requestModel.RegionType);
			var matchedRules = appliedRuleTypes.Intersect(rulesByRegion.Select(x => x.RuleType)).ToList();

			if (matchedRules.Any())
			{
				foreach (var ruleType in matchedRules)
				{
					var rule = _ruleFactory.GetRule(ruleType);
					var isSuccess = await rule.ApplyToRequest(requestModel);

					if (!isSuccess)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
