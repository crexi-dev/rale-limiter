using RateLimiter.DependencyInjection;
using TestApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddLogging();

builder.Services.AddRateLimiterServices(builder.Configuration);
builder.Services.AddTransient<RateLimitingMiddleware>();

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

app.UseMiddleware<RateLimitingMiddleware>();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

namespace TestApi
{
    public partial class Program
    {
    }
}
