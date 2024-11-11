using RateLimiter.Common.Exceptions;
using RateLimiter.Repository.TrafficRepository;
using RateLimiter.Services.Limiters;

namespace RateLimiter.UnitTests.LimiterTests;

public class FixedRequestsPerTimeSpanLimiterTests
{
    #region Initialize
    private readonly Mock<ITrafficRepository> _trafficRepositoryMock;
    private readonly FixedRequestsPerTimeSpanLimiter _limiter;

    public FixedRequestsPerTimeSpanLimiterTests()
    {
        _trafficRepositoryMock = new Mock<ITrafficRepository>();
        _limiter = new FixedRequestsPerTimeSpanLimiter(_trafficRepositoryMock.Object);
    }
    #endregion

    #region Limit Tests

    [Fact]
    public async Task Limit_ShouldNotThrowException_WhenUnderRateLimit()
    {
        var options = new RateLimitingOptions
        {
            MaxRequests = 5,
            TimeSpan = TimeSpan.FromMinutes(1)
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };

        _trafficRepositoryMock.Setup(repo => repo.CountTraffic(context.Token, context.Resource, options.TimeSpan))
                              .ReturnsAsync(3); // Below the MaxRequests

        var exception = await Record.ExceptionAsync(() => _limiter.Limit(options, context));

        Assert.Null(exception); // Expect no exception
    }

    [Fact]
    public async Task Limit_ShouldThrowRateLimitExceededException_WhenRateLimitExceeded()
    {
        var options = new RateLimitingOptions
        {
            MaxRequests = 5,
            TimeSpan = TimeSpan.FromMinutes(1)
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };

        _trafficRepositoryMock.Setup(repo => repo.CountTraffic(context.Token, context.Resource, options.TimeSpan))
                              .ReturnsAsync(5); // Equal to MaxRequests

        await Assert.ThrowsAsync<RateLimitExceededException>(() => _limiter.Limit(options, context));
    }

    [Fact]
    public async Task Limit_ShouldThrowArgumentException_WhenMaxRequestsIsZero()
    {
        var options = new RateLimitingOptions
        {
            MaxRequests = 0, // Invalid configuration
            TimeSpan = TimeSpan.FromMinutes(1)
        };
        var context = new RequestContext
        {
            Token = Guid.NewGuid().ToString(),
            Resource = "TestResource"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _limiter.Limit(options, context));
    }

    [Fact]
    public async Task Limit_ShouldThrowArgumentException_WhenTimeSpanIsZero()
    {
        var options = new RateLimitingOptions
        {
            MaxRequests = 5,
            TimeSpan = TimeSpan.Zero // Invalid configuration
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
