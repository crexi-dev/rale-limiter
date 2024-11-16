using M42.Data.Repositories;
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

namespace RateLimiter.Tests;

[TestFixture]
public class UsersDataServiceTest
{
    ServiceProvider _serviceProvider;

    private readonly IDataService<User> _usersDataService;
    private readonly IDataGeneratorService _dataGeneratorService;
    private readonly IConfigService _configService;

    public UsersDataServiceTest()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "RateLimitertDatabase"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<User>, UsersDataService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IConfigService, ConfigService>();

        _serviceProvider = services.BuildServiceProvider();

        _usersDataService = _serviceProvider.GetService<IDataService<User>>();
        _dataGeneratorService = _serviceProvider.GetService<IDataGeneratorService>();
        _configService = _serviceProvider.GetService<IConfigService>();

    }

    [SetUp]
    public void SetUp()
    {
        _configService.Reset();
    }


    [Test]
    public async void GetAllTest()
    {
        var seedUsers = new List<User>()
            {
                _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid()),
                _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid()),
                _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid())
            };

       await _configService.SeedUsers(seedUsers);

        try
        {
            var users = await _usersDataService.GetAllAsync();

            Assert.AreEqual(users.Count, 3);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    //[Test]
    //public void GetByIdTest()
    //{
    //    var usersDataService = _serviceProvider.GetService<IDataService<User>>();

    //    try
    //    {
    //        var users = usersDataService.SingleAsync();

    //        Assert.That(true, Is.False);
    //    }
    //    catch (NotImplementedException ex)
    //    {
    //        Assert.That(true, Is.True);
    //    }
    //}
    //[Test]
    //public void GetByIdentifierTest()
    //{
    //    var usersDataService = _serviceProvider.GetService<IDataService<User>>();

    //    try
    //    {
    //        var users = usersDataService.SingleAsync();

    //        Assert.That(true, Is.False);
    //    }
    //    catch (NotImplementedException ex)
    //    {
    //        Assert.That(true, Is.True);
    //    }
    //}
    //[Test]
    //public void AddTest()
    //{
    //    var usersDataService = _serviceProvider.GetService<IDataService<User>>();

    //    try
    //    {
    //        var users = usersDataService.AddAsync();

    //        Assert.That(true, Is.False);
    //    }
    //    catch (NotImplementedException ex)
    //    {
    //        Assert.That(true, Is.True);
    //    }
    //}
    //[Test]
    //public void UpdateTest()
    //{
    //    var usersDataService = _serviceProvider.GetService<IDataService<User>>();

    //    try
    //    {
    //        var users = usersDataService.UpdateAsync();

    //        Assert.That(true, Is.False);
    //    }
    //    catch (NotImplementedException ex)
    //    {
    //        Assert.That(true, Is.True);
    //    }
    //}
    //[Test]
    //public async void RemoveTest()
    //{
    //    var usersDataService = _serviceProvider.GetService<IDataService<User>>();

    //    try
    //    {
    //        var users = await usersDataService.RemoveAsync();

    //        Assert.That(true, Is.False);
    //    }
    //    catch (NotImplementedException ex)
    //    {
    //        Assert.That(true, Is.True);
    //    }
    //}
}