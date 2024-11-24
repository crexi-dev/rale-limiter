using Crexi.API.Common.RateLimiter;
using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.Auctions.API.Middleware.RateLimiter;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Crexi Auction API", Version = "v1" });

    // Add the security definition for Bearer tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add the security requirement to include the Bearer token globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddSingleton<IRateLimiter, RateLimiter>();
builder.Services.AddTransient<ITokenToClientConverter, TokenToClientConverter>();
builder.Services.AddTransient<ITokenExtractor, TokenExtractor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

RateLimiterConfigurator.ConfigureRateLimitingRules(app.Services.GetRequiredService<IRateLimiter>());
app.UseMiddleware<RateLimiterMiddleware>();

app.MapControllers();

app.Run();
