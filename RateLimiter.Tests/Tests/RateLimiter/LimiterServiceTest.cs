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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
    private static System.Timers.Timer aTimer;

    [SetUp]
    public void SetUp()
    {
        _configService.Reset();
    }
    [Test]
	public async Task AllowAccess_Test_1_NoLimiterRules()
	{
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var user = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US", false);
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

    [Test]
    public async Task AllowAccess_Test_2_PriorityUser()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            // Create users
            var priorityUser = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US", true);

            var users = new List<User>
            {
                priorityUser
            };
            await _configService.SeedUsers(users);

            // Create a several limiter rules
            var priorityUserRule = _dataGeneratorService.GenerateLimiterRule(id: 1, name: "Priority User", tokenSource: null, resourceStatusId: null, numPerTimespan: null, numSeconds: null, isPriorityUser: true);
            var resourceMaintenanceRule = _dataGeneratorService.GenerateLimiterRule(id: 2, name: "Resource Maintenance", tokenSource: null, resourceStatusId: Statuses.Maintenance.Id, numPerTimespan: 10, numSeconds: null, isPriorityUser: null);
            var usUserRule = _dataGeneratorService.GenerateLimiterRule(id: 3, name: "US User", tokenSource: "US", resourceStatusId: null, numPerTimespan: 5, numSeconds: 60, isPriorityUser: null);
            var euUserRule = _dataGeneratorService.GenerateLimiterRule(id: 4, name: "EU User", tokenSource: "EU", resourceStatusId: null, numPerTimespan: null, numSeconds: 20, isPriorityUser: null);

            var limiterRules = new List<LimiterRule>
            {
                priorityUserRule ,
                resourceMaintenanceRule ,
                usUserRule ,
                euUserRule
            };

            // Create a resource with limiter rules
            var resource = _dataGeneratorService.GenerateResource(1, "Resource1", Statuses.Normal, limiterRules);

            var resources = new List<Resource>
            {
                resource
            };
            await _configService.SeedResources(resources);

            // make 120 requests for the priority user against the resource.  all should be allowed..
            for (int i = 1; i <= 120; i++)
            {
                var newRequest = _dataGeneratorService.GenerateRequest(i, resource, priorityUser, "Request1", null);

                var result = await _limiterService.AllowAccess(newRequest);
                Assert.IsTrue(result);

                var request = await _requestDataService.SingleOrDefaultAsync(i);
                Assert.IsNotNull(request);
                Assert.AreEqual(request.WasHandled, true);

            }
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task AllowAccess_Test_3_USUser()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var usUser = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US", false);

            var users = new List<User>
            {
                usUser
            };
            await _configService.SeedUsers(users);

            // Create a several limiter rules
            var priorityUserRule = _dataGeneratorService.GenerateLimiterRule(id: 1, name: "Priority User", tokenSource: null, resourceStatusId: null, numPerTimespan: null, numSeconds: null, isPriorityUser: true);
            var resourceMaintenanceRule = _dataGeneratorService.GenerateLimiterRule(id: 2, name: "Resource Maintenance", tokenSource: null, resourceStatusId: Statuses.Maintenance.Id, numPerTimespan: 10, numSeconds: null, isPriorityUser: null);
            var usUserRule = _dataGeneratorService.GenerateLimiterRule(id: 3, name: "US User", tokenSource: "US", resourceStatusId: null, numPerTimespan: 5, numSeconds: 10, isPriorityUser: null);
            var euUserRule = _dataGeneratorService.GenerateLimiterRule(id: 4, name: "EU User", tokenSource: "EU", resourceStatusId: null, numPerTimespan: null, numSeconds: 5, isPriorityUser: null);

            var limiterRules = new List<LimiterRule>
            {
                priorityUserRule ,
                resourceMaintenanceRule ,
                usUserRule ,
                euUserRule
            };

            // Create a resource with limiter rules
            var resource = _dataGeneratorService.GenerateResource(1, "Resource1", Statuses.Normal, limiterRules);

            var resources = new List<Resource>
            {
                resource
            };
            await _configService.SeedResources(resources);

            // iterate 120 times and create a request for each of the users
            for (int i = 1; i <= 120; i++)
            {
                if (i == 6)
                {
                    string jeff = "";
                }
                var newRequest = _dataGeneratorService.GenerateRequest(i, resource, usUser, "Request1", null);

                var result = await _limiterService.AllowAccess(newRequest);

                if (i <= 5)
                {
                    Assert.IsTrue(result);
                }
                else
                {
                    Assert.IsFalse(result);
                }

            }
            Thread.Sleep(10000);  // wait 10 seconds

            // iterate another 120 times and create a request for each of the users
            for (int i = 121; i <= 240; i++)
            {
                var newRequest = _dataGeneratorService.GenerateRequest(i, resource, usUser, "Request1", null);

                var result = await _limiterService.AllowAccess(newRequest);

                if (i <= 125)
                {
                    Assert.IsTrue(result);
                }
                else
                {
                    Assert.IsFalse(result);
                }

            }

        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
}
