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
public class LimiterRulesDataServiceTest
{
    ServiceProvider _serviceProvider;

    private readonly IDataService<LimiterRule> _limiterRulesDataService;
    private readonly IDataGeneratorService _dataGeneratorService;
    private readonly IConfigService _configService;

    public LimiterRulesDataServiceTest()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "LimiterRulesDataServiceTest"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<LimiterRule>, LimiterRulesDataService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IConfigService, ConfigService>();

        _serviceProvider = services.BuildServiceProvider();

        _limiterRulesDataService = _serviceProvider.GetService<IDataService<LimiterRule>>();
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
        var limiterRule1 = _dataGeneratorService.GenerateLimiterRule(1, "US Users", "US", null, 3, 15);
        var limiterRule2 = _dataGeneratorService.GenerateLimiterRule(2, "EU Users", "EU", null, 1, 5);
        var limiterRule3 = _dataGeneratorService.GenerateLimiterRule(3, "CN Users", "CN", null, 2, 10);

        var seedLimiterRules = new List<LimiterRule>()
            {
                limiterRule1,
                limiterRule2,
                limiterRule3
            };

        await _configService.SeedLimiterRules(seedLimiterRules);

        try
        {
            var limiterRules = await _limiterRulesDataService.GetAllAsync();

            Assert.AreEqual(limiterRules.Count, 3);
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
            var limiterRules = await _limiterRulesDataService.FindAsync(searchCriteria);

            Assert.AreEqual(limiterRules.Count, -1); // should never reach this assertion
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);       // NotImplementedException is the expected result.
        }
    }
    [Test]
    public async Task GetByIdTest()     
    {
        var limiterRule1 = _dataGeneratorService.GenerateLimiterRule(1, "US Users", "US", null, 3, 15);
        var limiterRule2 = _dataGeneratorService.GenerateLimiterRule(2, "EU Users", "EU", null, 1, 5);
        var limiterRule3 = _dataGeneratorService.GenerateLimiterRule(3, "CN Users", "CN", null, 2, 10);

        var seedLimiterRules = new List<LimiterRule>()
            {
                limiterRule1,
                limiterRule2,
                limiterRule3
            };

        await _configService.SeedLimiterRules(seedLimiterRules);

        try
        {
            var retrievedLimiterRule = await _limiterRulesDataService.SingleAsync(limiterRule2.Id);

            Assert.AreEqual(retrievedLimiterRule.Name, limiterRule2.Name); 
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task GetByIdentifierTest()
    {
        var limiterRule1 = _dataGeneratorService.GenerateLimiterRule(1, "US Users", "US", null, 3, 15);
        var limiterRule2 = _dataGeneratorService.GenerateLimiterRule(2, "EU Users", "EU", null, 1, 5);
        var limiterRule3 = _dataGeneratorService.GenerateLimiterRule(3, "CN Users", "CN", null, 2, 10);

        var seedLimiterRules = new List<LimiterRule>()
            {
                limiterRule1,
                limiterRule2,
                limiterRule3
            };

        await _configService.SeedLimiterRules(seedLimiterRules);

        try
        {
            var retrievedLimiterRule = await _limiterRulesDataService.SingleAsync(limiterRule2.Identifier);

            Assert.AreEqual(retrievedLimiterRule.Id, limiterRule2.Id);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task AddTest()
    {
        var limiterRuleToAdd = _dataGeneratorService.GenerateLimiterRule(100, "US Users", "US", null, 3, 15);

        try
        {
            var limiterRule = await _limiterRulesDataService.AddAsync(limiterRuleToAdd);

            var retrievedLimiterRule = await _limiterRulesDataService.SingleAsync(limiterRuleToAdd.Id);

            Assert.AreEqual(limiterRuleToAdd.Name, retrievedLimiterRule.Name);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task UpdateTest()
    {
        var limiterRule1 = _dataGeneratorService.GenerateLimiterRule(1, "US Users", "US", null, 3, 15);
        var limiterRule2 = _dataGeneratorService.GenerateLimiterRule(2, "EU Users", "EU", null, 1, 5);
        var limiterRule3 = _dataGeneratorService.GenerateLimiterRule(3, "CN Users", "CN", null, 2, 10);

        var seedLimiterRules = new List<LimiterRule>()
            {
                limiterRule1,
                limiterRule2,
                limiterRule3
            };

        await _configService.SeedLimiterRules(seedLimiterRules);

        try
        {
            limiterRule2.Name = "Fred";

            var result = _limiterRulesDataService.UpdateAsync(limiterRule2.Id, limiterRule2);

            var updatedLimiterRule = await _limiterRulesDataService.SingleAsync(limiterRule2.Id);

            Assert.AreEqual(updatedLimiterRule.Name, "Fred");
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public async Task RemoveTest()
    {
        var limiterRule1 = _dataGeneratorService.GenerateLimiterRule(1, "US Users", "US", null, 3, 15);
        var limiterRule2 = _dataGeneratorService.GenerateLimiterRule(2, "EU Users", "EU", null, 1, 5);
        var limiterRule3 = _dataGeneratorService.GenerateLimiterRule(3, "CN Users", "CN", null, 2, 10);

        var seedLimiterRules = new List<LimiterRule>()
            {
                limiterRule1,
                limiterRule2,
                limiterRule3
            };

        await _configService.SeedLimiterRules(seedLimiterRules);

        try
        {
            var retrievedLimiterRules = await _limiterRulesDataService.GetAllAsync();
            Assert.AreEqual(retrievedLimiterRules.Count, 3);

            var limiterRules = await _limiterRulesDataService.RemoveAsync(limiterRule2.Id);

            retrievedLimiterRules = await _limiterRulesDataService.GetAllAsync();
            Assert.AreEqual(retrievedLimiterRules.Count, 2);

            var retrievedLimiterRule = await _limiterRulesDataService.SingleOrDefaultAsync(limiterRule2.Id);

            Assert.IsNull(retrievedLimiterRule);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
}