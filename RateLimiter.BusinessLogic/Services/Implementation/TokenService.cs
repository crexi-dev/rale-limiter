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
		private const string UniqueIndentifier = "UniqueIdentifierId";

		private readonly KeyGenerator _keyGenerator;
		private readonly TokenSettings _tokenSettings;

		public TokenService(
			KeyGenerator keyGenerator,
			IOptions<TokenSettings> options)
		{
			_keyGenerator = keyGenerator;
			_tokenSettings = options.Value;
		}

		public KeyGenerator KeyGenerator => _keyGenerator;
		public TokenSettings TokenSettings => _tokenSettings;

		public Task<string> GenerateTokenAsync(RegionTokenType type)
		{
			if (type == RegionTokenType.None)
			{
				throw new ArgumentException($"{nameof(RegionTokenType)} should not be set up in {RegionTokenType.None}.");
			}

			var token = new JwtSecurityToken(
				issuer: _tokenSettings.Issuer,
				audience: _tokenSettings.Audience,
				claims: new List<Claim>
				{
					new Claim(nameof(RegionTokenType), type.ToString()),
					new Claim(nameof(UniqueIndentifier), Guid.NewGuid().ToString())
				},
				signingCredentials: new SigningCredentials(_keyGenerator.GetKey(), _keyGenerator.SigningAlgorithm));

			return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
		}
	}
}
