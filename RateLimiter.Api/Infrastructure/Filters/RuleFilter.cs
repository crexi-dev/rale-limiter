using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Api.Infrastructure.Attributes;
using RateLimiter.BusinessLogic.Services;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Helpers;
using System.Security.Claims;

namespace RateLimiter.Api.Infrastructure.Filters
{
	public class RuleFilter : IAsyncActionFilter
	{
		private readonly IRuleFactory _ruleFactory;

		public RuleFilter(IRuleFactory ruleFactory)
		{
			_ruleFactory = ruleFactory;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var appliedRuleAttribute = context.ActionDescriptor.EndpointMetadata
				.OfType<AppliedRuleAttribute>()
				.FirstOrDefault();

			if (appliedRuleAttribute == null || !appliedRuleAttribute.RuleTypes.Any())
			{
				throw new Exception($"{nameof(AppliedRuleAttribute)} and {nameof(RuleFilter)} have been applied incorecctly.");
			}

			var ruleTypes = appliedRuleAttribute.RuleTypes.ToList();
			var (regionType, uniqueId) = GetClaimValues(context.HttpContext.User?.Claims);

			// TO DO

			await next();
		}

		private (RegionType regionType, string uniqueId) GetClaimValues(IEnumerable<Claim> claims)
		{
			var region = claims.FirstOrDefault(c => c.Type == nameof(CustomClaimTypes.Region)).Value;
			var identifierId = claims.FirstOrDefault(c => c.Type == nameof(CustomClaimTypes.UniqueIdentifier)).Value;

			return ((RegionType)Enum.Parse(typeof(RegionType), region), identifierId);
		}
	}
}
