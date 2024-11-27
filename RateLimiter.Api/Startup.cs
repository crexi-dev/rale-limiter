using Microsoft.AspNetCore.Mvc;
using RateLimiter.Api.Infrastructure;
using RateLimiter.Api.Infrastructure.Authentication;
using RateLimiter.Api.Infrastructure.ExeptionHandling;
using RateLimiter.Api.Infrastructure.Filters;
using RateLimiter.Api.Infrastructure.Swagger;
using System.Text.Json.Serialization;

namespace RateLimiter.Api
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddFilters();
			services.AddControllers(options =>
			{
				options.Filters.Add(new ServiceFilterAttribute(typeof(RequestLimitFilter)));

			}).AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

			services.AddAuthenticationConfiguration(Configuration);
			services.AddRateLimiterServices(Configuration);
			services.AddSwaggerServices();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwaggerServices();
			}
			else
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseErrorHandler();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
