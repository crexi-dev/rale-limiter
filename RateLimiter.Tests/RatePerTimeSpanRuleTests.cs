using System;
using System.Linq;
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

public class RatePerTimeSpanRuleTests
{
    private readonly Mock<IRequestsStorage> _requestsStorageMock;
    private readonly RatePerTimeSpanRule _rule;
    private readonly RatePerTimeSpanRuleSettings _settings = new() { IntervalInMinutes = 1, RequestsCount = 3 };

    public RatePerTimeSpanRuleTests()
    {
        _requestsStorageMock = new Mock<IRequestsStorage>();
        Mock<IOptionsSnapshot<RatePerTimeSpanRuleSettings>> optionsMock = new();
        optionsMock.Setup(o => o.Value).Returns(_settings);
        _rule = new RatePerTimeSpanRule(optionsMock.Object, _requestsStorageMock.Object);
    }

    [Fact]
    public void Validate_RequestsLimitNotReached_ShouldReturnTrue()
    {
        // Arrange
        var request = new Request(Guid.NewGuid(), RegionType.Us, DateTime.UtcNow);

        _requestsStorageMock
            .Setup(r => r.Get(request.Id))
            .Returns([new Request(request.Id, RegionType.Us, DateTime.UtcNow.AddMinutes(-1))]);

        // Act
        var result = _rule.Validate(request);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_RequestsLimitReached_ShouldReturnFalse()
    {
        // Arrange
        var request = new Request(Guid.NewGuid(), RegionType.Us, DateTime.UtcNow);

        var existingRequests = Enumerable.Range(0, _settings.RequestsCount)
            .Select(i => new Request(request.Id, RegionType.Us, DateTime.UtcNow.AddMinutes(-i)))
            .ToList();

        _requestsStorageMock.Setup(r => r.Get(request.Id)).Returns(existingRequests);

        // Act
        var result = _rule.Validate(request);

        // Assert
        result.Should().BeFalse();
    }
}