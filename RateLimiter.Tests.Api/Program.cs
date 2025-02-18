using RateLimiter.Config;
using RateLimiter.DependencyInjection;
using RateLimiter.Tests.Api.Middleware.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiting()
    .WithCustomDiscriminator<GeoTokenDiscriminator>()
    .WithConfiguration<RateLimiterConfiguration>(builder.Configuration.GetSection("RateLimiter"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.UseRateLimiting();

app.Run();
