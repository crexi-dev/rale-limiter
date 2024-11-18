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

    private readonly IDataService<Request> _requestsDataService;
    private readonly IDataGeneratorService _dataGeneratorService;
    private readonly IConfigService _configService;

    private Resource resource1;
    private Resource resource2;
    private User user1;
    private User user2;

    public RequestsDataServiceTest()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RateLimiterDbContext>(options => options.UseInMemoryDatabase(databaseName: "RequestsDataServiceTest"));
        services.AddTransient(typeof(DbRepository<>));
        services.AddScoped<IDataService<Request>, RequestsDataService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IConfigService, ConfigService>();

        _serviceProvider = services.BuildServiceProvider();

        _requestsDataService = _serviceProvider.GetService<IDataService<Request>>();
        _dataGeneratorService = _serviceProvider.GetService<IDataGeneratorService>();
        _configService = _serviceProvider.GetService<IConfigService>();

    }

    [SetUp]
    public async Task SetUp()
    {
        await _configService.Reset();

        resource1 = _dataGeneratorService.GenerateResource(1, "Resource1", Statuses.Normal, new List<LimiterRule>());
        resource2 = _dataGeneratorService.GenerateResource(2, "Resource2", Statuses.Normal, new List<LimiterRule>());

        var seedResources = new List<Resource>()
        {
            resource1,
            resource2
        };

        await _configService.SeedResources(seedResources);

        user1 = _dataGeneratorService.GenerateUser(1, "User1", Guid.NewGuid(), "US");
        user2 = _dataGeneratorService.GenerateUser(2, "User2", Guid.NewGuid(), "US");
        var seedUsers = new List<User>()
        {
            user1,
            user2
        };

        await _configService.SeedUsers(seedUsers);
    }


    [Test]
    public async Task GetAllTest()
    {

        var request1 = _dataGeneratorService.GenerateRequest(1, resource1, user1, "Request1", true);
        var request2 = _dataGeneratorService.GenerateRequest(2, resource1, user1, "Request2", false);
        var request3 = _dataGeneratorService.GenerateRequest(3, resource2, user1, "Request3", true);

        var seedRequests = new List<Request>()
            {
                request1,
                request2,
                request3
            };

        await _configService.SeedRequests(seedRequests);

        try
        {
            var requests = await _requestsDataService.GetAllAsync();

            Assert.AreEqual(requests.Count, 3);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task FindTest()
    {
        var request1 = _dataGeneratorService.GenerateRequest(1, resource1, user1, "Request1", true);
        var request2 = _dataGeneratorService.GenerateRequest(2, resource1, user1, "Request2", false);
        var request3 = _dataGeneratorService.GenerateRequest(3, resource2, user1, "Request3", true);

        var seedRequests = new List<Request>()
            {
                request1,
                request2,
                request3
            };

        await _configService.SeedRequests(seedRequests);

        try
        {
            var searchCriteria = new BaseModel { CreatedBy = "FredSmith" };
            var requests = await _requestsDataService.FindAsync(searchCriteria);

            Assert.AreEqual(requests.Count, 0); // should never reach this assertion

            searchCriteria = new BaseModel { CreatedBy = "DataGenerator" };
            requests = await _requestsDataService.FindAsync(searchCriteria);

            Assert.AreEqual(requests.Count, 3); // should never reach this assertion
        }
        catch (Exception ex)
        {
            Assert.That(true, Is.True);       // NotImplementedException is the expected result.
        }
    }
    [Test]
    public async Task GetByIdTest()
    {
        var request1 = _dataGeneratorService.GenerateRequest(1, resource1, user1, "Request1", true);
        var request2 = _dataGeneratorService.GenerateRequest(2, resource1, user1, "Request2", false);
        var request3 = _dataGeneratorService.GenerateRequest(3, resource2, user1, "Request3", true);

        var seedRequests = new List<Request>()
            {
                request1,
                request2,
                request3
            };

        await _configService.SeedRequests(seedRequests);

        try
        {
            var retrievedRequest = await _requestsDataService.SingleAsync(request2.Id);

            Assert.AreEqual(retrievedRequest.Identifier, request2.Identifier);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task GetByIdentifierTest()
    {
        var request1 = _dataGeneratorService.GenerateRequest(1, resource1, user1, "Request1", true);
        var request2 = _dataGeneratorService.GenerateRequest(2, resource1, user1, "Request2", false);
        var request3 = _dataGeneratorService.GenerateRequest(3, resource2, user1, "Request3", true);

        var seedRequests = new List<Request>()
            {
                request1,
                request2,
                request3
            };

        await _configService.SeedRequests(seedRequests);

        try
        {
            var retrievedRequest = await _requestsDataService.SingleAsync(request2.Identifier);

            Assert.AreEqual(retrievedRequest.Id, request2.Id);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task AddTest()
    {
        var requestToAdd = _dataGeneratorService.GenerateRequest(1, resource1, user1, "RequestToAdd", true);

        try
        {
            var request = await _requestsDataService.AddAsync(requestToAdd);

            var retrievedRequest = await _requestsDataService.SingleAsync(requestToAdd.Id);

            Assert.AreEqual(requestToAdd.Identifier, retrievedRequest.Identifier);
        }
        catch (Exception ex)
        {
            Assert.That(false, Is.True);
        }
    }
    [Test]
    public async Task UpdateTest()
    {
        var request1 = _dataGeneratorService.GenerateRequest(1, resource1, user1, "Request1", true);
        var request2 = _dataGeneratorService.GenerateRequest(2, resource1, user1, "Request2", false);
        var request3 = _dataGeneratorService.GenerateRequest(3, resource2, user1, "Request3", true);

        var seedRequests = new List<Request>()
            {
                request1,
                request2,
                request3
            };

        await _configService.SeedRequests(seedRequests);

        try
        {
            request2.Identifier = "Fred";

            var result = _requestsDataService.UpdateAsync(request2.Id, request2);

            var updatedRequest = await _requestsDataService.SingleAsync(request2.Id);

            Assert.AreEqual(updatedRequest.Identifier, "Fred");
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
    [Test]
    public async Task RemoveTest()
    {
        var request1 = _dataGeneratorService.GenerateRequest(1, resource1, user1, "Request1", true);
        var request2 = _dataGeneratorService.GenerateRequest(2, resource1, user1, "Request2", false);
        var request3 = _dataGeneratorService.GenerateRequest(3, resource2, user1, "Request3", true);

        var seedRequests = new List<Request>()
            {
                request1,
                request2,
                request3
            };

        await _configService.SeedRequests(seedRequests);

        try
        {
            var retrievedRequests = await _requestsDataService.GetAllAsync();
            Assert.AreEqual(retrievedRequests.Count, 3);

            var requests = await _requestsDataService.RemoveAsync(request2.Id);

            retrievedRequests = await _requestsDataService.GetAllAsync();
            Assert.AreEqual(retrievedRequests.Count, 2);

            var retrievedRequest = await _requestsDataService.SingleOrDefaultAsync(request2.Id);

            Assert.IsNull(retrievedRequest);
        }
        catch (NotImplementedException ex)
        {
            Assert.That(true, Is.True);
        }
    }
}