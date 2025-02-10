using RateLimiter.Config;
using RateLimiter.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddRateLimiting();

builder.Services.Configure<RateLimiterConfiguration>(
    builder.Configuration.GetSection("RateLimiter"));

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
