using M42.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Data.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services;
using System;

namespace RateLimiter.Tests;

[TestFixture]
public class UsersDataServiceTest
{
    ServiceProvider _serviceProvider;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "RateLimitertDatabase"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<User>, UsersDataService>();

        _serviceProvider = services.BuildServiceProvider();

    }
    [Test]
	public void GetTest()
	{
        var requestDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestDataService.Get();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
	}
}