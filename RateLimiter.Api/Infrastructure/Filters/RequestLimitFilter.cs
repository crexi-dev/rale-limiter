using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Api.Infrastructure.Attributes;
using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.RateLimiter;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Exceptions;
using RateLimiter.Core.Helpers;
using System.Security.Claims;

namespace RateLimiter.Api.Infrastructure.Filters
{
	public class RequestLimitFilter : IAsyncActionFilter
	{
		private readonly ILimitService _limitService;

		public RequestLimitFilter(ILimitService limitService)
		{
			_limitService = limitService;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var appliedRuleAttribute = context.ActionDescriptor.EndpointMetadata
				.OfType<AppliedRuleAttribute>()
				.FirstOrDefault();

			if (appliedRuleAttribute == null || !appliedRuleAttribute.RuleTypes.Any())
			{
				await next();
				return;
			}

			var (regionType, identifierId) = GetClaimValues(context.HttpContext.User.Claims);

			var isReachedLimit = await _limitService.IsRequestReachedLimit(
				appliedRuleAttribute.RuleTypes.ToList(),
				new RequestDto
				{
					Id = Guid.Parse(identifierId),
					RegionType = regionType,
					UrlPath = context.HttpContext.Request.Path,
					DateTime = DateTime.UtcNow,
				});

			if (isReachedLimit)
			{
				throw new TooManyRequestsException("The limit has been reached.");
			}

			await next();
		}

		private (RegionType regionType, string identifierId) GetClaimValues(IEnumerable<Claim> claims)
		{
			var region = claims.FirstOrDefault(c => c.Type == nameof(Constants.CustomClaimTypes.Region)).Value;
			var identifierId = claims.FirstOrDefault(c => c.Type == nameof(Constants.CustomClaimTypes.UniqueIdentifier)).Value;

			return ((RegionType)Enum.Parse(typeof(RegionType), region), identifierId);
		}
	}
}
