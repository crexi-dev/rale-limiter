using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Infrastructure;
using LoggerUnitTests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BlockedRequestsRepositoryTests
{
    private readonly Mock<ILogger<BlockedRequestsRepository>> _mockLogger;
    private readonly BlockedRequestsRepository _repository;

    public BlockedRequestsRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<BlockedRequestsRepository>>();
        _repository = new BlockedRequestsRepository(_mockLogger.Object);
    }

    [Fact]
    public async Task Get_ReturnsBlockedClientRecordIfExistsAndNotExpired()
    {
        // Arrange
        var key = "test-key";
        var blockExpires = DateTime.UtcNow.AddMinutes(10);

        await _repository.Add(key, blockExpires);

        // Act
        var result = await _repository.Get(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(key, result.RequestId);
        Assert.Equal(blockExpires.ToLocalTime(), result.BlockExpires);
    }

    [Fact]
    public async Task Get_ReturnsNullIfNotExists()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _repository.Get(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Get_DeletesRecordIfExpired()
    {
        // Arrange
        var key = "test-key";
        var blockExpires = DateTime.UtcNow.AddMinutes(-10);

        await _repository.Add(key, blockExpires);

        // Act
        var result = await _repository.Get(key);

        // Assert
        Assert.Null(result);
        ILogger<BlockedRequestsRepository> logger = _mockLogger.Object;
        _mockLogger.VerifyInformationWasCalled($"Removing block for {key}");
    }

    [Fact]
    public async Task Add_AddsNewRecord()
    {
        // Arrange
        var key = "test-key";
        var blockExpires = DateTime.UtcNow.AddMinutes(10);

        // Act
        await _repository.Add(key, blockExpires);

        // Assert
        var result = await _repository.Get(key);
        Assert.NotNull(result);
        Assert.Equal(key, result.RequestId);
        Assert.Equal(blockExpires.ToLocalTime(), result.BlockExpires);
    }

    [Fact]
    public async Task Update_UpdatesExistingRecord()
    {
        // Arrange
        var key = "test-key";
        var initialExpires = DateTime.UtcNow.AddMinutes(5);
        var updatedExpires = DateTime.UtcNow.AddMinutes(15);
        
        await _repository.Add(key, initialExpires);
        
        // Act
        await _repository.Update(key, updatedExpires);

        // Assert
        var result = await _repository.Get(key);
        Assert.NotNull(result);
        Assert.Equal(updatedExpires.ToLocalTime(), result.BlockExpires);
    }

    [Fact]
    public async Task Delete_RemovesExistingRecord()
    {
        // Arrange
        var key = "test-key";
        var blockExpires = DateTime.UtcNow.AddMinutes(10);

        await _repository.Add(key, blockExpires);

        // Act
        await _repository.Delete(key);

        // Assert
        var result = await _repository.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_RaisesEventWhenRecordRemoved()
    {
        // Arrange
        var key = "test-key";
        var blockExpires = DateTime.UtcNow.AddMinutes(10);
        var eventRaised = false;

        await _repository.Add(key, blockExpires);

        _repository.RequestBlockDeleted += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        await _repository.Delete(key);

        // Assert
        Assert.True(eventRaised);
    }
}