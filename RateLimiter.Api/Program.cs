using Microsoft.AspNetCore.Mvc;
using RateLimiter.Api.ApiFilters;
using RateLimiter.Api.Extensions;
using RateLimiter.Contracts;
using RateLimiter.Storage;
using RateLimiter.RateLimitRules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ServiceFilterAttribute(typeof(RateLimitFilter)));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSettings(builder.Configuration);
builder.Services.AddRateLimitRules();

builder.Services.AddScoped<RateLimitFilter>();
builder.Services.AddScoped<IRateLimitExecutor, RateLimitRulesExecutor>();
builder.Services.AddSingleton<IRequestsStorage, RequestsStorage>();

builder.Services.AddMemoryCache();

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