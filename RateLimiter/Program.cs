using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiter.ViewModel;
using Services.Common.Repositories;
using System.Text.Json;
using RateLimiter.MiddleWares;
using Services.Common.Configurations;
using Services.Common.RateLimiters;
using Services.Common.RateLimitRules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// These below initialization mock in-memory data simulating dynamic data retrieval from stateful services such as database, redis, etc.
builder.Services.AddSingleton<IDataRepository<User>, InMemoryDataRepository<User>>(_ =>
{
    var repo = new InMemoryDataRepository<User>([
        new User { Id = Guid.Parse("296035ED-532E-46C9-8172-60806CC86B50"), Name = "micky mouse", Email = "micky@disney.com", Address = "micky house, LA, CA" },
        new User { Id = Guid.Parse("D9FB7E5B-7498-4F8C-8996-6A6537FD2A46"), Name = "minnie mouse", Email = "minnie@disney.com", Address = "minnie house, Anaheim, CA" },
        new User { Id = Guid.Parse("E396E6B5-660E-4949-9E4E-8CBCCA707001"), Name = "donald duck", Email = "donald@disney.com", Address = "Donald house, Orlando, FL" },
        new User { Id = Guid.Parse("F14A7950-08CA-46E1-8080-AFFEC1A3BE8E"), Name = "goofy", Email = "goofy@disney.com", Address = "goofy house, Tokyo, Japan" }
    ]);

    return repo;
});

builder.Services.AddSingleton<IDataRepository<Property>, InMemoryDataRepository<Property>>(_ =>
{
    var repo = new InMemoryDataRepository<Property>([
        new Property { Id = Guid.Parse("4F3C7A4B-6C43-4686-9F31-6F1AF5E17951"), Address = "Disneyland, Anaheim, CA" },
        new Property { Id = Guid.Parse("74470EEB-1F5D-42EF-AA00-C8C2794278F7"), Address = "Disney World, Orlando, FL" },
        new Property { Id = Guid.Parse("B6B6DB4F-4594-46F0-A3AC-288BDDB1ACBD"), Address = "Disneyland, Tokyo, Japan" },
        new Property { Id = Guid.Parse("E402198E-A2D0-4868-B52A-FDA85DF264E9"), Address = "Disneyland, Paris, France" }
    ]);

    return repo;
});

builder.Services.AddSingleton<IRuleRepository, InMemoryRuleRepository>(_ =>
{
    // Load and deserialize JSON configuration
    var jsonConfig = File.ReadAllText("rateLimitConfig.json");
    var configs = JsonSerializer.Deserialize<List<RuleConfig>>(jsonConfig);
    return new InMemoryRuleRepository(configs);
});

builder.Services.AddSingleton<IRateLimitRuleFactory, RateLimitRuleFactory>();
builder.Services.AddSingleton<IRuleConfigLoader, RuleConfigLoader>();
builder.Services.AddSingleton<RuleConfigLoader>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RuleConfigLoader>());
builder.Services.AddSingleton<IRateLimiter, DynamicRateLimiter>();

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<AddHeaderParameterOperationFilter>(); // Register the header filter
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.EnableFilter();
        c.RoutePrefix = "swagger"; // Makes swagger the default route
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<RateLimitMiddleware>();

app.Run();

