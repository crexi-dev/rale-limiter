using M42.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Data.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class ConfigServiceTest
{
    ServiceProvider _serviceProvider;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "RateLimitertDatabase"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<Request>, RequestsDataService>();
        services.AddScoped<IDataService<Resource>, ResourcesDataService>();
        services.AddScoped<IDataService<User>, UsersDataService>();

        _serviceProvider = services.BuildServiceProvider();

    }
    [Test]
	public void GetTest()
	{
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestsDataService.Get();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
	}
}
