using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.EU
{
	public class LastCallTimeRule : IRuleService
	{
		public RegionType RegionType => RegionType.EU;
		public RuleType RuleType => RuleType.TimeSpanPassedSinceLastCall;

		public Task ApplyRule()
		{
			throw new NotImplementedException();
		}
	}
}
