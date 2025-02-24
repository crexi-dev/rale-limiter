using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services;
using RateLimiter.Services.Rules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 

// Add Health Checks 
builder.Services.AddHealthChecks();

// 🔹 Add Swagger (with API Key Authentication)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rate Limiter API",
        Version = "v1",
        Description = "API with per-client rate limiting"
    });

    // 🔹 Add API Key Authentication (X-Client-Id)
    c.AddSecurityDefinition("X-Client-Id", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Client-Id",
        Type = SecuritySchemeType.ApiKey,
        Description = "Client ID required for rate limiting"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "X-Client-Id" }
            },
            Array.Empty<string>()
        }
    });
});

//Register Rate Limiter Services
var rateLimiterManager = new RateLimiterManager();

var clientRegions = new List<ClientRegionConfig>
{
    new ClientRegionConfig { ClientId = "ClientA", Region = "US" },
    new ClientRegionConfig { ClientId = "ClientB", Region = "EU" }
};

var clientRateLimits = clientRegions.Select(client => new ClientRateLimitConfig
{
    ClientId = client.ClientId,
    ResourceLimits = new List<ResourceRateLimitConfig>
    {
        new ResourceRateLimitConfig
        {
            Resource = "/api/RateLimitTest/resource1",
            Rules = client.Region == "US"
                ? new List<IRateLimitRule> { new FixedWindowRateLimit(5, TimeSpan.FromSeconds(10)) }
                : new List<IRateLimitRule> { new SlidingWindowRateLimit(1, TimeSpan.FromSeconds(5)) }
        },
        new ResourceRateLimitConfig
        {
            Resource = "/api/RateLimitTest/resource2",
            Rules = client.Region == "US"
                ? new List<IRateLimitRule> { new FixedWindowRateLimit(10, TimeSpan.FromSeconds(20)) }
                : new List<IRateLimitRule> { new SlidingWindowRateLimit(1, TimeSpan.FromSeconds(10)) }
        }
    }
}).ToList();

rateLimiterManager.AddRateLimitRules(clientRateLimits);
builder.Services.AddSingleton(rateLimiterManager);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

//Good when deploying to ECS
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
    }
});


app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<RateLimiterMiddleware>(); // Rate limiter middleware first
app.MapControllers(); // Map API routes

app.Run();
