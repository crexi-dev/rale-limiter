using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Helpers;
using RateLimiter.Core.Settings;
using System.Text;

namespace RateLimiter.Api.Infrastructure.Authentication
{
	public static class Authentication
	{
		public static void AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					var settings = configuration.GetSection(nameof(TokenSettings)).Get<TokenSettings>();
					var secretKey = configuration.GetValue<string>(Constants.SecretKey);

					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						RequireSignedTokens = true,
						RequireExpirationTime = false,
						ValidIssuer = settings.Issuer,
						ValidAudience = settings.Audience,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
					};

					options.Events = new JwtBearerEvents
					{
						OnTokenValidated = context =>
						{
							var claimsPrincipal = context.Principal;

							var regionClaim = claimsPrincipal.FindFirst(nameof(Constants.CustomClaimTypes.Region));
							var uniqueIdentifierClaim = claimsPrincipal.FindFirst(nameof(Constants.CustomClaimTypes.UniqueIdentifier));

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

							if(!Guid.TryParse(uniqueIdentifier, out _))
							{
								context.Fail(GetClaimValueErrorMessage(nameof(Constants.CustomClaimTypes.UniqueIdentifier)));
								return Task.CompletedTask;
							}

							if (!Enum.TryParse(regionType, out RegionType type))
							{
								context.Fail(GetClaimValueErrorMessage(nameof(Constants.CustomClaimTypes.Region)));
							}

							return Task.CompletedTask;
						}
					};
				});
		}

		private static string GetClaimValueErrorMessage(string claimType)
			=> $"Invalid value of claim: {claimType}.";
	}
}
