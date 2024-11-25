using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.Api.Infrastructure.Attributes;
using RateLimiter.Api.Infrastructure.Filters;
using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/target")]
	public class TargetController
	{
		[HttpGet]
		[Route("limited-info")]
		[AppliedRule(RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall)]
		[ServiceFilter(typeof(RuleFilter))]
		public Task GetLimitedInfo() => Task.CompletedTask;

		[HttpGet]
		[Route("full-info")]
		public Task GetFullInfo() => Task.CompletedTask;
	}
}
