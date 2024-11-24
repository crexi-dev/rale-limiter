namespace RateLimiter.Api.Infrastructure.Authentication
{
	public static class JwtBearerExtensions
	{
		public static void AddJwtBearerConfiguration(this IServiceCollection services)
			=> services.ConfigureOptions<ConfigureJwtBearerOptions>();
	}
}
