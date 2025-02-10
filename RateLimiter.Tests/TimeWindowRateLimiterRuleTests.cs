using System;
using NUnit.Framework;
using RateLimiter.Exceptions;
using RateLimiter.Implementations;

namespace RateLimiter.Tests;

[TestFixture]
public class TimeWindowRateLimiterRuleTests
{
    [Test]
    public void Validate_EnoughTime_Ok()
    {
        // Arrange
        const int delay = 10;
        const int maxRequests = 3;
        var token = Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2, delay / 2, delay * 2);
        var limiter = new TimeWindowRateLimiterRule(TimeSpan.FromSeconds(delay), maxRequests);
        // Act, Assert
        Assert.DoesNotThrow(() => limiter.Validate(token, previousRequests));
    }

    [Test]
    public void Validate_NotEnoughTime_ThrowsException()
    {
        // Arrange
        const int delay = 10;
        const int maxRequests = 3;
        var token = Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2, delay / 2, delay / 2);
        var limiter = new TimeWindowRateLimiterRule(TimeSpan.FromSeconds(delay), maxRequests);
        // Act, Assert
        Assert.Throws<RateLimitException>(() => limiter.Validate(token, previousRequests));
    }

    [Test]
    public void Validate_NotEnoughTimeWithSelector_ThrowsException()
    {
        // Arrange
        const int delay = 10;
        const int maxRequests = 3;
        var token = "US-" + Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2, delay / 2, delay / 2);
        var limiter = new TimeWindowRateLimiterRule(TimeSpan.FromSeconds(delay), maxRequests, Moq.PrefixSelector("US-"));
        // Act, Assert
        Assert.Throws<RateLimitException>(() => limiter.Validate(token, previousRequests));
    }

    [Test]
    public void Validate_NotApplicable_Ok()
    {
        // Arrange
        const int delay = 10;
        const int maxRequests = 3;
        var token = "EU-" + Guid.NewGuid().ToString("N");
        var previousRequests = Moq.RequestsDates(delay / 2, delay / 2, delay / 2);
        var limiter = new TimeWindowRateLimiterRule(TimeSpan.FromSeconds(delay), maxRequests, Moq.PrefixSelector("US-"));
        // Act, Assert
        Assert.DoesNotThrow(() => limiter.Validate(token, previousRequests));
    }
}