namespace RateLimiter.UnitTests.LimiterTests;

using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RateLimiter.Repository.TrafficRepository;
using RateLimiter.Services.Limiters;
using RateLimiter.Common.Exceptions;

public class TimeSpanSinceLastRequestLimiterTests
{
    #region Initialize

    private readonly Mock<ITrafficRepository> _trafficRepositoryMock;
    private readonly TimeSpanSinceLastRequestLimiter _limiter;

    public TimeSpanSinceLastRequestLimiterTests()
    {
        _trafficRepositoryMock = new Mock<ITrafficRepository>();
        _limiter = new TimeSpanSinceLastRequestLimiter(_trafficRepositoryMock.Object);
    }

    #endregion

    #region Limit Tests

    [Fact]
    public async Task Limit_ShouldNotThrowException_WhenTimeSinceLastRequestIsGreaterThanConfiguredTimeSpan()
    {
        var options = new RateLimitingOptions
        {
            TimeSpan = TimeSpan.FromSeconds(30)
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };
        var lastTraffic = new Traffic(context.Resource, context.Token) { Time = DateTime.UtcNow.AddMinutes(-1) };

        _trafficRepositoryMock.Setup(repo => repo.GetLatestTraffic(context.Token, context.Resource))
                              .ReturnsAsync(lastTraffic); // Last request was more than 30 seconds ago

        var exception = await Record.ExceptionAsync(() => _limiter.Limit(options, context));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Limit_ShouldThrowRateLimitExceededException_WhenTimeSinceLastRequestIsLessThanConfiguredTimeSpan()
    {
        var options = new RateLimitingOptions
        {
            TimeSpan = TimeSpan.FromMinutes(1)
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };
        var lastTraffic = new Traffic(context.Resource, context.Token) { Time = DateTime.UtcNow.AddSeconds(-30) };

        _trafficRepositoryMock.Setup(repo => repo.GetLatestTraffic(context.Token, context.Resource))
                              .ReturnsAsync(lastTraffic); // Last request was only 30 seconds ago

        await Assert.ThrowsAsync<RateLimitExceededException>(() => _limiter.Limit(options, context));
    }

    [Fact]
    public async Task Limit_ShouldNotThrowException_WhenNoPreviousTrafficExists()
    {
        var options = new RateLimitingOptions
        {
            TimeSpan = TimeSpan.FromMinutes(1)
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };

        _trafficRepositoryMock.Setup(repo => repo.GetLatestTraffic(context.Token, context.Resource)).ReturnsAsync((Traffic?)null);

        var exception = await Record.ExceptionAsync(() => _limiter.Limit(options, context));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Limit_ShouldThrowArgumentException_WhenConfiguredTimeSpanIsZero()
    {
        var options = new RateLimitingOptions
        {
            TimeSpan = TimeSpan.Zero
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _limiter.Limit(options, context));
    }

    #endregion
}

