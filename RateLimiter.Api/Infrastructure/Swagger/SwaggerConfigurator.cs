using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag.Generation.Processors.Security;
using NSwag;

namespace RateLimiter.Api.Infrastructure.Swagger
{
	public static class SwaggerConfigurator
	{
		public static void AddSwaggerServices(this IServiceCollection services)
		{
			services.AddOpenApiDocument(options =>
			{
				options.Title = "Rate Limiter API Doc";
				options.Version = "1.0";

				options.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
				{
					Type = OpenApiSecuritySchemeType.Http,
					Scheme = "bearer",
					Description = "Enter the JWT token in the format: {token}"
				});

				options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
			});
		}

		public static void UseSwaggerServices(this IApplicationBuilder app)
		{
			app.UseOpenApi();
			app.UseSwaggerUi(x =>
			{
				x.DocExpansion = "list";
			});
		}
	}
}
