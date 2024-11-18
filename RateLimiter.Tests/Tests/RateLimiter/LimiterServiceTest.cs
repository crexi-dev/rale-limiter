using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Data.CodeValues;
using RateLimiter.Data.Contexts;
using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models;
using RateLimiter.Data.Repositories;
using RateLimiter.Data.Services;
using RateLimiter.Interfaces;
using RateLimiter.Services;
using RateLimiter.Tests.Interfaces;
using RateLimiter.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class LimiterServiceTest
{
    ServiceProvider _serviceProvider;

    private readonly ILimiterService _limiterService;
    private readonly IDataGeneratorService _dataGeneratorService;
    private readonly IConfigService _configService;
    private readonly IDataService<Request> _requestDataService;

    public LimiterServiceTest()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "LimiterServiceTest"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<Request>, RequestsDataService>();
        services.AddScoped<IDataService<Resource>, ResourcesDataService>();
        services.AddScoped<IDataService<User>, UsersDataService>();
        services.AddScoped<ILimiterService, LimiterService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IConfigService, ConfigService>();

        _serviceProvider = services.BuildServiceProvider();
        _dataGeneratorService = _serviceProvider.GetService<IDataGeneratorService>();
        _configService = _serviceProvider.GetService<IConfigService>();
        _requestDataService = _serviceProvider.GetService <IDataService<Request>>();
        _limiterService = _serviceProvider.GetService<ILimiterService>();
    }

    [SetUp]
    public void SetUp()
    {
        // do I want to clear the db for each test?  probably...
    }
    [Test]
	public async Task AllowAccess_Test_1()
	{
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var user = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
            var users = new List<User> { user };
            await _configService.SeedUsers(users);

            var limiterRules = new List<LimiterRule>();
            var resource = _dataGeneratorService.GenerateResource(1, "Resource1", Statuses.Normal, limiterRules);
            var resources = new List<Resource> { resource };
            await _configService.SeedResources(resources);

            // handle 10 requests.  all should be allowed through.
            for (int i = 1; i <= 10; i++)
            {
                var newRequest = _dataGeneratorService.GenerateRequest(i, resource, user, "Request1", null);

                var result = await _limiterService.AllowAccess(newRequest);
                Assert.IsTrue(result);

                var request = await _requestDataService.SingleOrDefaultAsync(i);
                Assert.IsNotNull(request);
                Assert.AreEqual(request.WasHandled, true);
            }

            var requests = await _requestDataService.GetAllAsync();

            Assert.IsNotNull(requests);
            Assert.AreEqual(requests.Count(), 10);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
	}
}
