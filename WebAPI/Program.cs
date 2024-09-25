using RateLimiter;
using RateLimiter.Attributes;
using RateLimiter.Infrastructure;
using WebAPI.Infrastructure;
using WebAPI.Models;

namespace WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped(typeof(IProductInventoryManager<Widget, Guid>), typeof(WidgetInventoryManager));
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            builder.Services.AddSingleton(typeof(IProductRepository<Widget, Guid>), typeof(TestWidgetRepository));
        builder.Services.AddLogging();
        builder.Services.AddScoped<RateLimiterAttribute>();
        builder.Services.ConfigureServices();

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