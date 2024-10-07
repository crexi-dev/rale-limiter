using RateLimiter.Api.Middlewares;
using RateLimiter.Api.Services;
using RateLimiter.Geo;
using RateLimiter.Storages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register IGeoService and IRateLimitStore
builder.Services.AddSingleton<IGeoService, SimpleGeoService>();
builder.Services.AddSingleton<IRateLimitStore, InMemoryStore>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _=app.UseSwagger();
    _=app.UseSwaggerUI();
}

// Use the rate-limiting middleware
app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();