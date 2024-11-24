using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.Api.Models.Request;
using RateLimiter.BusinessLogic.Services;

namespace RateLimiter.Api.Controllers
{
	/// <summary>
	/// Auth
	/// </summary>
	[Authorize]
	[ApiController]
	[Route("api/auth")]
	public class AuthController
	{
		private readonly ITokenService _tokenService;

		public AuthController(ITokenService tokenService)
		{
			_tokenService = tokenService;
		}

		/// <summary>
		/// Generate Token for US/EU
		/// </summary>
		/// <param name="model">TokenRequestModel</param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		[Route("generate-token")]
		public async Task<string> GenerateToken([FromBody] CreateTokenRequestModel model)
			=> await _tokenService.GenerateTokenAsync(model.Type);
	}
}
