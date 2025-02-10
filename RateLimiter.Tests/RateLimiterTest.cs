using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Exceptions;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [Test]
    public void LimitRequestsForToken_Ok()
    {
        // Arrange
        var token = Guid.NewGuid().ToString("N");
        var limiter = new Implementations.RateLimiter([Moq.Rule(true)],
                                                      Moq.RepositoryFromDelays(),
                                                      Mock.Of<ILogger<Implementations.RateLimiter>>());
        // Act, Assert
        Assert.DoesNotThrow(() => limiter.LimitRequestsForToken(token));
    }

    [Test]
    public void LimitRequestsForToken_Fails()
    {
        // Arrange
        var token = Guid.NewGuid().ToString("N");
        var limiter = new Implementations.RateLimiter([Moq.Rule(false)],
                                                      Moq.RepositoryFromDelays(),
                                                      Mock.Of<ILogger<Implementations.RateLimiter>>());
        // Act, Assert
        Assert.Throws<RateLimitException>(() => limiter.LimitRequestsForToken(token));
    }
}