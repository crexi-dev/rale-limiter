using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.RateLimitRules;
using Xunit;

namespace RateLimiter.Tests;

public class RateLimitRulesExecutorTests
{
    private readonly Mock<IRateLimitRule> _rule1Mock;
    private readonly Mock<IRateLimitRule> _rule2Mock;
    private readonly RateLimitRulesExecutor _executor;

    private readonly Request _request = new(Guid.NewGuid(), RegionType.Eu, DateTime.UtcNow);

    public RateLimitRulesExecutorTests()
    {
        _rule1Mock = new Mock<IRateLimitRule>();
        _rule2Mock = new Mock<IRateLimitRule>();

        _rule1Mock.Setup(r => r.RuleType).Returns(RuleType.TimeSpanSinceLastCall);
        _rule2Mock.Setup(r => r.RuleType).Returns(RuleType.RequestPerTimeSpan);

        _executor = new RateLimitRulesExecutor(new List<IRateLimitRule> { _rule1Mock.Object, _rule2Mock.Object });
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void Execute_ShouldReturnExpectedResult(bool rule1, bool rule2, bool expectedResult)
    {
        // Arrange
        var ruleTypes = new[] { RuleType.TimeSpanSinceLastCall, RuleType.RequestPerTimeSpan };

        _rule1Mock.Setup(r => r.Validate(_request)).Returns(rule1);
        _rule2Mock.Setup(r => r.Validate(_request)).Returns(rule2);
    
        // Act
        var result = _executor.Execute(ruleTypes, _request);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Execute_NoRules_ShouldReturnTrue()
    {
        // Arrange
        var ruleTypes = Array.Empty<RuleType>();

        // Act
        var result = _executor.Execute(ruleTypes, _request);

        // Assert
        result.Should().BeTrue();
    }
}