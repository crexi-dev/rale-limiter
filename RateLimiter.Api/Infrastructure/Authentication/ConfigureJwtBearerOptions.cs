using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RateLimiter.BusinessLogic.Services;

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
					IssuerSigningKey = tokenService.KeyGenerator.GetKey(),
				};
			}
		}
	}
}
