using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RateLimiter.BusinessLogic.Services;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Helpers;

namespace RateLimiter.Api.Infrastructure.Authentication
{
	public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ConfigureJwtBearerOptions(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		public void Configure(string? name, JwtBearerOptions options) => Configure(options);

		public void Configure(JwtBearerOptions options)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var serviceProvider = scope.ServiceProvider;
				var tokenService = serviceProvider.GetRequiredService<ITokenService>();

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					RequireSignedTokens = true,
					RequireExpirationTime = false,
					ValidIssuer = tokenService.TokenSettings.Issuer,
					ValidAudience = tokenService.TokenSettings.Audience,
					IssuerSigningKey = tokenService.KeyGeneratorService.GetKey(),
				};

				options.Events = new JwtBearerEvents
				{
					OnTokenValidated = context =>
					{
						var claimsPrincipal = context.Principal;

						var regionClaim = claimsPrincipal.FindFirst(nameof(CustomClaimTypes.Region));
						var uniqueIdentifierClaim = claimsPrincipal.FindFirst(nameof(CustomClaimTypes.UniqueIdentifier));

						if (regionClaim == null || uniqueIdentifierClaim == null)
						{
							context.Fail($"Required custom claims have been missed.");
							return Task.CompletedTask;
						}

						var regionType = regionClaim.Value;
						var uniqueIdentifier = uniqueIdentifierClaim.Value;

						if (string.IsNullOrEmpty(regionType) || string.IsNullOrEmpty(uniqueIdentifier))
						{
							context.Fail("Custom claim values are invalid.");
							return Task.CompletedTask;
						}

						if (!Enum.TryParse(regionType, out RegionType type))
						{
							context.Fail($"Invalid value of claim: {nameof(CustomClaimTypes.Region)}.");
						}

						return Task.CompletedTask;
					}
				};
			}
		}
	}
}