using RateLimiter.BusinessLogic.Models;
using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.RateLimiter.Rules
{
	public interface IRuleService
	{
		public RegionType RegionType { get; }
		public RuleType RuleType { get; }
		Task<bool> ApplyToRequest(RequestDto requestModel);
	}
}
