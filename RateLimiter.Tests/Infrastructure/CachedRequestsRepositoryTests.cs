using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using Xunit;

public class CachedRequestsRepositoryTests
{
    private readonly CachedRequestsRepository _repository;
    private readonly Mock<ILogger<CachedRequestsRepository>> _loggerMock;

    public CachedRequestsRepositoryTests()
    {
        _loggerMock = new Mock<ILogger<CachedRequestsRepository>>();
        _repository = new CachedRequestsRepository(_loggerMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsRecord_WhenKeyExists()
    {
        // Arrange
        var key = "testKey";
        var requestDetails = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));
        await _repository.Add(key, requestDetails);

        // Act
        var result = await _repository.Get(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(requestDetails.RequestId, result.RequestId);
    }

    [Fact]
    public async Task Get_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Act
        var result = await _repository.Get("nonExistingKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMany_ReturnsRequests_WhenKeyExists()
    {
        // Arrange
        var key = "testKey";
        var requestDetails = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));
        await _repository.Add(key, requestDetails);

        // Act
        var result = await _repository.GetMany(key);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(requestDetails.RequestId, result.First().RequestId);
    }

    [Fact]
    public async Task GetMany_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Act
        var result = await _repository.GetMany("nonExistingKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Add_InsertsNewRequest()
    {
        // Arrange
        var key = "testKey";
        var requestDetails = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));

        // Act
        await _repository.Add(key, requestDetails);
        var result = await _repository.Get(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(requestDetails.RequestId, result.RequestId);
        Assert.Contains(requestDetails, result.Requests);
    }

    [Fact]
    public async Task Update_UpdatesExistingRequest()
    {
        // Arrange
        var key = "testKey";
        var initialRequest = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));
        await _repository.Add(key, initialRequest);

        var updatedRequest = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));

        // Act
        await _repository.Update(key, updatedRequest);
        var result = await _repository.Get(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(initialRequest.RequestId, result.RequestId);
        Assert.Contains(updatedRequest, result.Requests);
    }

    [Fact]
    public async Task Delete_RemovesRequest()
    {
        // Arrange
        var key = "testKey";
        var requestDetails = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));
        await _repository.Add(key, requestDetails);

        // Act
        await _repository.Delete(key);
        var result = await _repository.Get(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleBlockDeleted_DeletesRequest()
    {
        // Arrange
        var key = "testKey";
        var requestDetails = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now.AddHours(1));
        await _repository.Add(key, requestDetails);

        var eventArgs = new DeletedRequestBlockEventArgs(key);

        // Act
        _repository.HandleBlockDeleted(this, eventArgs);
        await Task.Delay(500); 
        var result = await _repository.Get(key);

        // Assert
        Assert.Null(result);
    }
}


