using System;
using NUnit.Framework;
using RateLimiter.Exceptions;
using RateLimiter.Implementations;

namespace RateLimiter.Tests;

[TestFixture]
public class DelayRateLimiterRuleTests
{
    [Test]
    public void Validate_EnoughTime_Ok()
    {
        // Arrange
        const int delay = 10;
        var token = Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay + delay / 2, delay * 2);
        var limiter = new DelayRateLimiterRule(TimeSpan.FromSeconds(delay));
        // Act, Assert
        Assert.DoesNotThrow(() => limiter.Validate(token, previousRequests));
    }

    [Test]
    public void Validate_NotEnoughTime_ThrowsException()
    {
        // Arrange
        const int delay = 10;
        var token = Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2);
        var limiter = new DelayRateLimiterRule(TimeSpan.FromSeconds(delay));
        // Act, Assert
        Assert.Throws<RateLimitException>(() => limiter.Validate(token, previousRequests));
    }

    [Test]
    public void Validate_NotEnoughTimeWithSelector_ThrowsException()
    {
        // Arrange
        const int delay = 10;
        var token = "US-" + Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2);
        var limiter = new DelayRateLimiterRule(TimeSpan.FromSeconds(delay), Moq.PrefixSelector("US-"));
        // Act, Assert
        Assert.Throws<RateLimitException>(() => limiter.Validate(token, previousRequests));
    }

    [Test]
    public void Validate_NotApplicable_Ok()
    {
        // Arrange
        const int delay = 10;
        var token = "EU-" + Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2);
        var limiter = new DelayRateLimiterRule(TimeSpan.FromSeconds(delay), Moq.PrefixSelector("US-"));
        // Act, Assert
        Assert.DoesNotThrow(() => limiter.Validate(token, previousRequests));
    }
}