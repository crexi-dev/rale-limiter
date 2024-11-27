using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.DataAccess.Repository;

namespace RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.USA
{
	public class RequestPerTimeRule : RequestPerTimeSpanRuleBase, IRuleService
	{
		public RegionType RegionType => RegionType.US;
		public RuleType RuleType => RuleType.RequestPerTimeSpan;

		public RequestPerTimeRule(IRequestRepository requestRepository) 
			: base(requestRepository) { }
	}
}
