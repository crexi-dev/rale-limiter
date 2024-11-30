using FluentValidation;

namespace RateLimiter.Api.Infrastructure.Validators
{
	public static class ValidatorConfigurator
	{
		public static void AddValidators(this IServiceCollection services)
			=> services.AddValidatorsFromAssemblyContaining<TokenRequestValidator>();
	}
}
