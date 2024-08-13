using RateLimiter.Examples;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.RegisterRateLimiterPolicies();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseAuthorization();

        app.UseMiddleware<PostRateLimiterMiddleware>();
        app.MapControllers();


        app.Run();
    }
}