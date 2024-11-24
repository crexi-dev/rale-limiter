using Microsoft.Extensions.Configuration;
using RateLimiter.BusinessLogic.Services;
using RateLimiter.BusinessLogic.Services.Implementation;
using RateLimiter.Core.Helpers;
using RateLimiter.Core.Settings;

namespace RateLimiter.Api.Infrastructure
{
	public static class IocConfig
	{
		public static void AddRateLimiterServices(this IServiceCollection services)
		{
			AddBussinessLogicServices(services);
			AddDataAccessLayerRepositories(services);
		}

		private static void AddBussinessLogicServices(this IServiceCollection services)
		{ }

		private static void AddDataAccessLayerRepositories(this IServiceCollection services)
		{ }

		public static void AddTokenConfigurationServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<TokenSettings>(configuration.GetSection(nameof(TokenSettings)));
			services.AddSingleton(provider =>
			{
				var secretKey = configuration.GetValue<string>("GeneratorSecretKey");
				return new KeyGenerator(secretKey);
			});

			services.AddScoped<ITokenService, TokenService>();
		}
	}
}
