using M42.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models.Filter;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services;
using RateLimiter.Tests.Interfaces;
using RateLimiter.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RequestsDataServiceTest
{
    ServiceProvider _serviceProvider;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "RateLimitertDatabase"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IConfigService, ConfigService>();    
        services.AddScoped<IDataService<Request>, RequestsDataService>();

        _serviceProvider = services.BuildServiceProvider();

        
    }
    protected void DataSetup()
    {
        var dataGeneratorService = _serviceProvider.GetService<IDataGeneratorService>();
        var configService = _serviceProvider.GetService<IConfigService>();

        var users = new List<User>()
        {
            dataGeneratorService.GenerateUser("ResourceUser1", Guid.NewGuid())
        };
        configService.SeedUsers(users);

        var resources = new List<Resource>()
        {
            dataGeneratorService.GenerateResource("Resource1", CodeValues.Statuses.Single(x => x.Identifier == ""))
        };

        configService.SeedResources(resources);
    }
  


    [Test]
    public void GetAllTest()
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
    [Test]
    public void FindTest()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requestSearchFilter = new BaseModel();

            var requests = requestsDataService.FindAsync(requestSearchFilter);

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public void GetByIdTest()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestsDataService.SingleAsync();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public void GetByIdentifierTest()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestsDataService.SingleAsync();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public void AddTest()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestsDataService.AddAsync();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public void UpdateTest()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestsDataService.UpdateAsync();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public void RemoveTest()
    {
        var requestsDataService = _serviceProvider.GetService<IDataService<Request>>();

        try
        {
            var requests = requestsDataService.RemoveAsync();

            Assert.That(true, Is.False);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
}
