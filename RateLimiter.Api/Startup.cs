using Microsoft.AspNetCore.Authentication.JwtBearer;
using RateLimiter.Api.Infrastructure;
using RateLimiter.Api.Infrastructure.Authentication;
using RateLimiter.Api.Infrastructure.ExeptionHandling;
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
			services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

			services.AddTokenConfigurationServices(Configuration);
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
			services.ConfigureOptions<ConfigureJwtBearerOptions>();

			services.AddRateLimiterServices();
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
