using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.DataAccess.Repository;

namespace RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.EU
{
	public class LastCallTimeRule : LastCallTimeRuleBase, IRuleService
	{
		public RegionType RegionType => RegionType.EU;
		public RuleType RuleType => RuleType.TimeSpanPassedSinceLastCall;

		public LastCallTimeRule(IRequestRepository requestRepository)
			: base(requestRepository) { }
	}
}
