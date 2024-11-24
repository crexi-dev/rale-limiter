using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiter.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/target")]
	public class TargetController : ControllerBase
	{
		[HttpGet]
		[Route("test-endpoint")]
		public Task<string> TestEndpoint()
		{
			var a = User.Claims.ToList();

			return Task.FromResult("Ok");
		}
	}
}
