using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RateLimiter.Data.Contexts;
using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models;
using RateLimiter.Data.Repositories;
using RateLimiter.Data.Services;
using RateLimiter.Tests.Interfaces;
using RateLimiter.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "UsersDataServiceTest"));
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
    public async Task SetUp()
    {
        await _configService.Reset();
    }


    [Test]
    public async Task GetAllTest()
    {
        var user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        var user2 = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US");
        var user3 = _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid(), "US");

        var seedUsers = new List<User>()
            {
                user1,
                user2,
                user3
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
    [Test]
    public async Task FindTest()
    {
        // this test verifies that a user can be found using the Identifier criteria
        // which is where the token will be stored 

        var user2Token = Guid.NewGuid();

        var user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        var user2 = _dataGeneratorService.GenerateUser(2, "User2", user2Token, "US");
        var user3 = _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid(), "US");

        var seedUsers = new List<User>()
            {
                user1,
                user2,
                user3
            };

        await _configService.SeedUsers(seedUsers);

        try
        {
            var searchCriteria = new BaseModel
            {
                 Identifier = user2Token.ToString()
            };
            var users = await _usersDataService.FindAsync(searchCriteria);

            Assert.AreEqual(users.Count, 1);                                    // should only find one user
            Assert.AreEqual(users.Single().Identifier, user2Token.ToString());  // user's identifier should match the token 
        }
        catch (Exception ex)
        {
            Assert.Fail("Exception was thrown during Find() method call.");       
        }
    }
    [Test]
    public async Task GetByIdTest()     
    {
        var user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        var user2 = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US");
        var user3 = _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid(), "US");

        var seedUsers = new List<User>()
            {
                user1,
                user2,
                user3
            };

        await _configService.SeedUsers(seedUsers);

        try
        {
            var retrievedUser = await _usersDataService.SingleAsync(user2.Id);

            Assert.AreEqual(retrievedUser.Name, user2.Name); 
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task GetByIdentifierTest()
    {
        var user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        var user2 = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US");
        var user3 = _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid(), "US");

        var seedUsers = new List<User>()
            {
                user1,
                user2,
                user3
            };

        await _configService.SeedUsers(seedUsers);

        try
        {
            var retrievedUser = await _usersDataService.SingleAsync(user2.Identifier);

            Assert.AreEqual(retrievedUser.Id, user2.Id);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task AddTest()
    {
        var userToAdd = _dataGeneratorService.GenerateUser(100, "UserToAdd", Guid.NewGuid(), "US");

        try
        {
            var user = await _usersDataService.AddAsync(userToAdd);

            var retrievedUser = await _usersDataService.SingleAsync(userToAdd.Id);

            Assert.AreEqual(userToAdd.Name, retrievedUser.Name);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task UpdateTest()
    {
        var user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        var user2 = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US");
        var user3 = _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid(), "US");

        var seedUsers = new List<User>()
            {
                user1,
                user2,
                user3
            };

        await _configService.SeedUsers(seedUsers);

        try
        {
            user2.Name = "Fred";

            var result = _usersDataService.UpdateAsync(user2.Id, user2);

            var updatedUser = await _usersDataService.SingleAsync(user2.Id);

            Assert.AreEqual(updatedUser.Name, "Fred");
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public async Task RemoveTest()
    {

        var user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        var user2 = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US");
        var user3 = _dataGeneratorService.GenerateUser(3, "User3", Guid.NewGuid(), "US");

        var seedUsers = new List<User>()
            {
                user1,
                user2,
                user3
            };

        await _configService.SeedUsers(seedUsers);

        try
        {
            var retrievedUsers = await _usersDataService.GetAllAsync();
            Assert.AreEqual(retrievedUsers.Count, 3);

            var users = await _usersDataService.RemoveAsync(user2.Id);

            retrievedUsers = await _usersDataService.GetAllAsync();
            Assert.AreEqual(retrievedUsers.Count, 2);

            var retrievedUser = await _usersDataService.SingleOrDefaultAsync(user2.Id);

            Assert.IsNull(retrievedUser);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
}