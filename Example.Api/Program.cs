using System.Reflection;
using Example.Api.Helpers;
using Example.Api.Managers;
using Example.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rlm = new RateLimitManager();
builder.Services.AddSingleton<IRateLimitManager>(rlm);

builder.Services.RegisterControllers(rlm);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<RequestRateMiddleware>();

app.MapControllers();

app.Run();
