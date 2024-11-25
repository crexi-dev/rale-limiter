using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services
{
	public interface IRuleService
	{
		public RegionType RegionType { get; }
		public RuleType RuleType { get; }
		Task ApplyRule();
	}
}