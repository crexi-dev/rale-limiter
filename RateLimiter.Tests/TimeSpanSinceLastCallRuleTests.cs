using System;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.RateLimitRules;
using RateLimiter.RateLimitSettings;
using Xunit;

namespace RateLimiter.Tests;

public class TimeSpanSinceLastCallRuleTests
{
    private readonly Mock<IRequestsStorage> _requestsStorageMock = new();
    private readonly Mock<IOptionsSnapshot<TimeSpanSinceLastCallRuleSettings>> _optionsMock = new();
    private readonly TimeSpanSinceLastCallRule _rule;
    private readonly TimeSpanSinceLastCallRuleSettings _settings = new() { MinimumIntervalInMinutes = 1, Region = "Eu" };

    public TimeSpanSinceLastCallRuleTests()
    {
        _rule = new TimeSpanSinceLastCallRule(_optionsMock.Object, _requestsStorageMock.Object);
    }

    [Fact]
    public void Validate_FirstRequest_ShouldReturnTrue()
    {
        // Arrange
        var request = new Request(Guid.NewGuid(), RegionType.Eu, DateTime.UtcNow);
        _optionsMock.Setup(o => o.Value).Returns(_settings);
        _requestsStorageMock
            .Setup(r => r.Get(request.Id))
            .Returns([]);

        // Act
        var result = _rule.Validate(request);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_RequestInIntervalExists_ShouldReturnFalse()
    {
        // Arrange
        var request = new Request(Guid.NewGuid(), RegionType.Eu, DateTime.UtcNow);
        var previousRequest = new Request(request.Id, RegionType.Eu, DateTime.UtcNow.AddMinutes(-0.5));
        _optionsMock.Setup(o => o.Value).Returns(_settings);
        _requestsStorageMock
            .Setup(r => r.Get(request.Id))
            .Returns([previousRequest]);

        // Act
        var result = _rule.Validate(request);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_RegionIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new Request(Guid.NewGuid(), RegionType.Us, DateTime.UtcNow);
        _optionsMock.Setup(o => o.Value)
            .Returns(new TimeSpanSinceLastCallRuleSettings { Region = "invalid-region" });

        // Act
        Action act = () => _rule.Validate(request);
        
        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}
