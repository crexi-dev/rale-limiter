using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.USA
{
	public class RequestPerTimeRule : IRuleService
	{
		public RegionType RegionType => RegionType.US;
		public RuleType RuleType => RuleType.RequestPerTimeSpan;

		public Task ApplyRule()
		{
			throw new NotImplementedException();
		}
	}
}
