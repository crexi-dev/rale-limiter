using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Helpers;
using RateLimiter.Core.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RateLimiter.BusinessLogic.Services.Implementation
{
	public class TokenService : ITokenService
	{
		private readonly IKeyGeneratorService _keyGenerator;
		private readonly TokenSettings _tokenSettings;

		public TokenService(
			IKeyGeneratorService keyGenerator,
			IOptions<TokenSettings> options)
		{
			_keyGenerator = keyGenerator;
			_tokenSettings = options.Value;
		}

		public Task<string> GenerateTokenAsync(RegionType type)
		{
			var token = new JwtSecurityToken(
				issuer: _tokenSettings.Issuer,
				audience: _tokenSettings.Audience,
				claims: new List<Claim>
				{
					new Claim(nameof(Constants.CustomClaimTypes.Region), type.ToString()),
					new Claim(nameof(Constants.CustomClaimTypes.UniqueIdentifier), Guid.NewGuid().ToString())
				},
				signingCredentials: new SigningCredentials(_keyGenerator.GetKey(), _keyGenerator.SigningAlgorithm));

			return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
		}
	}
}
