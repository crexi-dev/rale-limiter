using RateLimiter.Config;
using RateLimiter.DependencyInjection;
using RateLimiter.Tests.Api.Middleware.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddRateLimiting()
    .WithCustomDiscriminator<GeoTokenDiscriminator>();

builder.Services.Configure<RateLimiterConfiguration>(
    builder.Configuration.GetSection("RateLimiter"));

//builder.Services.AddKeyedSingleton<IProvideADiscriminator, GeoTokenDiscriminator>(nameof(GeoTokenDiscriminator));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseRateLimiting();

app.Run();
