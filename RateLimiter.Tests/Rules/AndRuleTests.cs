using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Rules;
using Moq;
using Xunit;
using RateLimiter.Contracts;

public class AndRuleTests
{
    private readonly Mock<IRateRule> _baseRuleMock;
    private readonly Mock<IRateRule> _andRuleMock;
    private readonly Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> _overrideFunc;

    public AndRuleTests()
    {
        _baseRuleMock = new Mock<IRateRule>();
        _andRuleMock = new Mock<IRateRule>();
        _overrideFunc = context => Task.FromResult(new AllowRequestResult(true, "Override"));
    }

    [Fact]
    public async Task Evaluate_WithBothRules_ShouldCombineResults()
    {
        // Arrange
        _baseRuleMock.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
            .ReturnsAsync(new AllowRequestResult(true, "Base"));
        _andRuleMock.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
            .ReturnsAsync(new AllowRequestResult(false, "And"));

        var andRule = new AndRule(_baseRuleMock.Object, _andRuleMock.Object, "TestRule");

        // Act
        var result = await andRule.Evaluate(new List<RequestDetails>().AsEnumerable());

        // Assert
        Assert.False(result.AllowRequest);
        Assert.Equal("Base -> And", result.Reason);
    }

    [Fact]
    public async Task Evaluate_WithOverrideFunc_ShouldCombineResults()
    {
        // Arrange
        _baseRuleMock.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
            .ReturnsAsync(new AllowRequestResult(true, "Base"));

        var andRule = new AndRule(_baseRuleMock.Object, _overrideFunc, "TestRule");

        // Act
        var result = await andRule.Evaluate(new List<RequestDetails>());

        // Assert
        Assert.True(result.AllowRequest);
        Assert.Equal("Base -> Override", result.Reason);
    }

    [Fact]
    public async Task Evaluate_WithBaseRuleFalse_ShouldReturnFalse()
    {
        // Arrange
        _baseRuleMock.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
            .ReturnsAsync(new AllowRequestResult(false, "Base"));
        _andRuleMock.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
            .ReturnsAsync(new AllowRequestResult(true, "And"));

        var andRule = new AndRule(_baseRuleMock.Object, _andRuleMock.Object, "TestRule");

        // Act
        var result = await andRule.Evaluate(new List<RequestDetails>());

        // Assert
        Assert.False(result.AllowRequest);
        Assert.Equal("Base -> And", result.Reason);
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var andRule = new AndRule(_baseRuleMock.Object, _andRuleMock.Object, "TestRule");

        // Act
        var result = andRule.ToString();

        // Assert
        Assert.Equal("AndRule::TestRule", result);
    }
}


