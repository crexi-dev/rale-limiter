﻿using M42.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RateLimiter.Data;
using RateLimiter.Data.Interfaces;
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
public class ResourcesDataServiceTest
{
    ServiceProvider _serviceProvider;

    private readonly IDataService<Resource> _resourcesDataService;
    private readonly IDataGeneratorService _dataGeneratorService;
    private readonly IConfigService _configService;

    public ResourcesDataServiceTest()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "ResourcesTest"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<Resource>, ResourcesDataService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IConfigService, ConfigService>();

        _serviceProvider = services.BuildServiceProvider();

        _resourcesDataService = _serviceProvider.GetService<IDataService<Resource>>();
        _dataGeneratorService = _serviceProvider.GetService<IDataGeneratorService>();
        _configService = _serviceProvider.GetService<IConfigService>();

    }

    [SetUp]
    public async Task SetUp()
    {
        await _configService.Reset();
    }


    [Test]
    public async Task GetAllTest()
    {
        var resource1 = _dataGeneratorService.GenerateResource(1, "Resource1", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource2 = _dataGeneratorService.GenerateResource(2, "Resource2", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource3 = _dataGeneratorService.GenerateResource(3, "Resource3", CodeValues.Statuses.Single(x => x.Name == "Normal"));

        var seedResources = new List<Resource>()
            {
                resource1,
                resource2,
                resource3
            };

        await _configService.SeedResources(seedResources);

        try
        {
            var resources = await _resourcesDataService.GetAllAsync();

            Assert.AreEqual(resources.Count, 3);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task FindTest()
    {
        try
        {
            var searchCriteria = new BaseModel();
            var resources = await _resourcesDataService.FindAsync(searchCriteria);

            Assert.AreEqual(resources.Count, -1); // should never reach this assertion
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);       // NotImplementedException is the expected result.
        }
    }
    [Test]
    public async Task GetByIdTest()
    {
        var resource1 = _dataGeneratorService.GenerateResource(1, "Resource1", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource2 = _dataGeneratorService.GenerateResource(2, "Resource2", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource3 = _dataGeneratorService.GenerateResource(3, "Resource3", CodeValues.Statuses.Single(x => x.Name == "Normal"));

        var seedResources = new List<Resource>()
            {
                resource1,
                resource2,
                resource3
            };

        await _configService.SeedResources(seedResources);

        try
        {
            var retrievedResource = await _resourcesDataService.SingleAsync(resource2.Id);

            Assert.AreEqual(retrievedResource.Name, resource2.Name);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task GetByIdentifierTest()
    {
        var resource1 = _dataGeneratorService.GenerateResource(1, "Resource1", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource2 = _dataGeneratorService.GenerateResource(2, "Resource2", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource3 = _dataGeneratorService.GenerateResource(3, "Resource3", CodeValues.Statuses.Single(x => x.Name == "Normal"));

        var seedResources = new List<Resource>()
            {
                resource1,
                resource2,
                resource3
            };

        await _configService.SeedResources(seedResources);

        try
        {
            var retrievedResource = await _resourcesDataService.SingleAsync(resource2.Identifier);

            Assert.AreEqual(retrievedResource.Id, resource2.Id);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task AddTest()
    { 
        var resourceToAdd = _dataGeneratorService.GenerateResource(1, "ResourceToAdd", CodeValues.Statuses.Single(x => x.Name == "Normal"));

        try
        {
            var resource = await _resourcesDataService.AddAsync(resourceToAdd);

            var retrievedResource = await _resourcesDataService.SingleAsync(resourceToAdd.Id);

            Assert.AreEqual(resourceToAdd.Name, retrievedResource.Name);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task UpdateTest()
    {
        var resource1 = _dataGeneratorService.GenerateResource(1, "Resource1", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource2 = _dataGeneratorService.GenerateResource(2, "Resource2", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource3 = _dataGeneratorService.GenerateResource(3, "Resource3", CodeValues.Statuses.Single(x => x.Name == "Normal"));

        var seedResources = new List<Resource>()
            {
                resource1,
                resource2,
                resource3
            };

        await _configService.SeedResources(seedResources);

        try
        {
            resource2.Name = "Fred";

            var result = _resourcesDataService.UpdateAsync(resource2.Id, resource2);

            var updatedResource = await _resourcesDataService.SingleAsync(resource2.Id);

            Assert.AreEqual(updatedResource.Name, "Fred");
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public async Task RemoveTest()
    {

        var resource1 = _dataGeneratorService.GenerateResource(1, "Resource1", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource2 = _dataGeneratorService.GenerateResource(2, "Resource2", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var resource3 = _dataGeneratorService.GenerateResource(3, "Resource3", CodeValues.Statuses.Single(x => x.Name == "Normal"));
        var seedResources = new List<Resource>()
            {
                resource1,
                resource2,
                resource3
            };

        await _configService.SeedResources(seedResources);

        try
        {
            var retrievedResources = await _resourcesDataService.GetAllAsync();
            Assert.AreEqual(retrievedResources.Count, 3);

            var resources = await _resourcesDataService.RemoveAsync(resource2.Id);

            retrievedResources = await _resourcesDataService.GetAllAsync();
            Assert.AreEqual(retrievedResources.Count, 2);

            var retrievedResource = await _resourcesDataService.SingleOrDefaultAsync(resource2.Id);

            Assert.IsNull(retrievedResource);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
}