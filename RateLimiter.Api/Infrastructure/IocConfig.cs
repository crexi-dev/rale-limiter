using RateLimiter.Api.Infrastructure.Filters;
using RateLimiter.BusinessLogic.Services;
using RateLimiter.BusinessLogic.Services.Implementation;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Factory;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.EU;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.USA;
using RateLimiter.BusinessLogic.Services.RateLimiter;
using RateLimiter.BusinessLogic.Services.RateLimiter.Factory;
using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
using RateLimiter.Core.Helpers;
using RateLimiter.Core.Settings;
using RateLimiter.DataAccess.Repository;
using RateLimiter.DataAccess.Repository.Implementation;

namespace RateLimiter.Api.Infrastructure
{
	public static class IocConfig
	{
		public static void AddFilters(this IServiceCollection services)
			=> services.AddScoped<RequestLimitFilter>();

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
				var secretKey = configuration.GetValue<string>(Constants.SecretKey);
				return new KeyGeneratorService(secretKey);
			});

			services.AddScoped<ITokenService, TokenService>();
		}

		private static void AddBussinessLogicServices(this IServiceCollection services)
		{
			AddRulesServices(services);
			services.AddScoped<ILimitService, LimitService>();
		}
		private static void AddRulesServices(this IServiceCollection services)
		{
			services.AddScoped<IRuleFactory, RuleFactory>();
			services.AddScoped<IRuleService, LastCallTimeRule>();
			services.AddScoped<IRuleService, RequestPerTimeRule>();
		}

		private static void AddDataAccessLayerRepositories(this IServiceCollection services)
			=> services.AddSingleton<IRequestRepository, RequestRepository>();
	}
}
