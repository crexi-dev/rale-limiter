using FluentAssertions;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

public class TimeSpanRuleTests
{
    [Fact]
    public async Task Allows_Requests_After_TimeSpan()
    {
        // Arrange
        var rule = new TimeSpanRule(requiredTimeSpan: TimeSpan.FromSeconds(1));
        var clientToken = "client1";
        var resource = "resource1";

        // Act
        var result1 = await rule.IsRequestAllowed(clientToken, resource);
        Thread.Sleep(1100); // Wait more than 1 second
        var result2 = await rule.IsRequestAllowed(clientToken, resource);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    [Fact]
    public async Task Denies_Requests_Within_TimeSpan()
    {
        // Arrange
        var rule = new TimeSpanRule(requiredTimeSpan: TimeSpan.FromSeconds(2));
        var clientToken = "client1";
        var resource = "resource1";

        // Act
        await rule.IsRequestAllowed(clientToken, resource);
        Thread.Sleep(1000); // Wait less than 2 seconds
        var result = await rule.IsRequestAllowed(clientToken, resource);

        // Assert
        result.Should().BeFalse(); // Should be denied
    }

    #region Edge Case Tests

    [Fact]
    public async Task TimeSpanRule_Should_Handle_Zero_TimeSpan()
    {
        // Arrange
        var rule = new TimeSpanRule(requiredTimeSpan: TimeSpan.Zero);
        var clientToken = "edgeClient";
        var resource = "resource1";

        // Act
        var result1 = await rule.IsRequestAllowed(clientToken, resource);
        var result2 = await rule.IsRequestAllowed(clientToken, resource); // Immediate call

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue(); // Should be allowed since the time span is zero
    }

    #endregion
}