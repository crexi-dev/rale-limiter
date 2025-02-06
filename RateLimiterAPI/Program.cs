using System.Net;

namespace RateLimiterAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();
            // Add services to the container.
            builder.Services.AddSingleton< RateLimiter.Domain.Interfaces.IRequestReqository, RateLimiter.Infrastructure.RequestRepository>();
            builder.Services.AddScoped< RateLimiter.Domain.Interfaces.IRuleRunner, RateLimiter.Domain.RuleRunner>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
