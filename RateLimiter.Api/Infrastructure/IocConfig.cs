using RateLimiter.Api.Infrastructure.Filters;
using RateLimiter.BusinessLogic.Services;
using RateLimiter.BusinessLogic.Services.Implementation;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Factory;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.EU;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.USA;
using RateLimiter.Core.Settings;

namespace RateLimiter.Api.Infrastructure
{
	public static class IocConfig
	{
		public static void AddFilters(this IServiceCollection services)
		{
			services.AddScoped<RuleFilter>();
		}

		public static void AddRateLimiterServices(this IServiceCollection services, IConfiguration configuration)
		{
			AddTokenConfigurationServices(services, configuration);
			AddBussinessLogicServices(services);
			AddDataAccessLayerRepositories(services);
		}

		private static void AddTokenConfigurationServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<TokenSettings>(configuration.GetSection(nameof(TokenSettings)));
			services.AddSingleton<IKeyGeneratorService>(provider =>
			{
				var secretKey = configuration.GetValue<string>("GeneratorSecretKey");
				return new KeyGeneratorService(secretKey);
			});

			services.AddScoped<ITokenService, TokenService>();
		}

		private static void AddBussinessLogicServices(this IServiceCollection services)
		{
			services.AddScoped<IRuleFactory, RuleFactory>();
			services.AddScoped<IRuleService, LastCallTimeRule>();
			services.AddScoped<IRuleService, RequestPerTimeRule>();
		}

		private static void AddDataAccessLayerRepositories(this IServiceCollection services)
		{ }
	}
}
