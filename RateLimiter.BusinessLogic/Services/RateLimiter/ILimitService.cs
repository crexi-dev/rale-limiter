using RateLimiter.BusinessLogic.Models;
using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services.RateLimiter
{
	public interface ILimitService
	{
		Task<bool> IsRequestReachedLimit(List<RuleType> appliedRuleTypes, RequestDto requestModel);
	}
}
