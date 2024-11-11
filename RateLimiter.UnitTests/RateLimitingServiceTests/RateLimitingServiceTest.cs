
using RateLimiter.Common.Enum;
using RateLimiter.Common.Exceptions;
using RateLimiter.Repository.TrafficRepository;
using RateLimiter.Services.Limiters;
using RateLimiter.Services.RateLimitingService;
using RateLimiter.Services.RequestContextService;

namespace RateLimiter.UnitTests.RateLimitingServiceTests;

public class RateLimitingServiceTests
{
    #region Initialize
    private readonly Mock<IRequestContextService> _contextServiceMock;
    private readonly Mock<ITrafficRepository> _trafficRepositoryMock;
    private readonly Mock<FixedRequestsPerTimeSpanLimiter> _fixedRequestsLimiterMock;
    private readonly Mock<TimeSpanSinceLastRequestLimiter> _timeSinceLastRequestLimiterMock;
    private readonly RateLimitingService _rateLimitingService;

    public RateLimitingServiceTests()
    {
        _contextServiceMock = new Mock<IRequestContextService>();
        _trafficRepositoryMock = new Mock<ITrafficRepository>();
        _fixedRequestsLimiterMock = new Mock<FixedRequestsPerTimeSpanLimiter>(_trafficRepositoryMock.Object);
        _timeSinceLastRequestLimiterMock = new Mock<TimeSpanSinceLastRequestLimiter>(_trafficRepositoryMock.Object);

        _rateLimitingService = new RateLimitingService(
            _contextServiceMock.Object,
            _trafficRepositoryMock.Object,
            _fixedRequestsLimiterMock.Object,
            _timeSinceLastRequestLimiterMock.Object
        );
    }
    #endregion

    #region HandleRequest Tests
    [Fact]
    public async Task HandleRequest_ShouldRecordTraffic_WhenCalled()
    {
        var context = new RequestContext
        {
            Resource = "TestResource",
            Token = Guid.NewGuid().ToString(),
            Options = []
        };

        _contextServiceMock.Setup(service => service.GetRequestContext()).ReturnsAsync(context);
        _trafficRepositoryMock.Setup(repo => repo.RecordTraffic(It.IsAny<Traffic>())).Returns(Task.CompletedTask);

        await _rateLimitingService.HandleRequest();

        _trafficRepositoryMock.Verify(repo => repo.RecordTraffic(It.Is<Traffic>(t => t.Resource == context.Resource && t.Token == context.Token)), Times.Once);
    }

    [Fact]
    public async Task HandleRequest_ShouldCallHandleOption_ForEachRateLimitingOption()
    {
        var context = new RequestContext
        {
            Resource = "TestResource",
            Token = Guid.NewGuid().ToString(),
            Options = [
                new RateLimitingOptions(),
                new RateLimitingOptions()
            ]
        };

        _contextServiceMock.Setup(service => service.GetRequestContext()).ReturnsAsync(context);

        await _rateLimitingService.HandleRequest();

        _trafficRepositoryMock.Verify(repo => repo.RecordTraffic(It.Is<Traffic>(t => t.Resource == context.Resource && t.Token == context.Token)), Times.Once);
    }
    #endregion

    #region HandleOption Tests
    [Fact]
    public async Task HandleOption_ShouldNotApplyLimiter_WhenOptionDoesNotApplyToOriginCountry()
    {
        var option = new RateLimitingOptions { ApplicableCountryCodes = ["US"] };
        var context = new RequestContext { Options = [option], OriginIsoCountryCode = "CA" };

        _contextServiceMock.Setup(service => service.GetRequestContext()).ReturnsAsync(context);

        await _rateLimitingService.HandleRequest();

        _fixedRequestsLimiterMock.Verify(limiter => limiter.Limit(It.IsAny<RateLimitingOptions>(), It.IsAny<RequestContext>()), Times.Never);
        _timeSinceLastRequestLimiterMock.Verify(limiter => limiter.Limit(It.IsAny<RateLimitingOptions>(), It.IsAny<RequestContext>()), Times.Never);
    }

    [Fact]
    public async Task HandleOption_ShouldApplyFixedRequestsLimiter_WhenRequestsPerTimespanMethodIsSet()
    {
        var option = new RateLimitingOptions { Method = RateLimitingMethod.RequestsPerTimespan };
        var context = new RequestContext { Options = [option] };

        _contextServiceMock.Setup(service => service.GetRequestContext()).ReturnsAsync(context);
        _fixedRequestsLimiterMock.Setup(limiter => limiter.Limit(option, context));

        await _rateLimitingService.HandleRequest();

        _fixedRequestsLimiterMock.Verify(limiter => limiter.Limit(option, context), Times.Once);
    }

    [Fact]
    public async Task HandleOption_ShouldApplyTimeSinceLastRequestLimiter_WhenTimeSpanSinceLastRequestMethodIsSet()
    {
        var option = new RateLimitingOptions { Method = RateLimitingMethod.TimeSpanSinceLastRequest };
        var context = new RequestContext { Options = [option] };

        _contextServiceMock.Setup(service => service.GetRequestContext()).ReturnsAsync(context);
        _timeSinceLastRequestLimiterMock.Setup(limiter => limiter.Limit(option, context));

        await _rateLimitingService.HandleRequest();

        _timeSinceLastRequestLimiterMock.Verify(limiter => limiter.Limit(option, context), Times.Once);
    }

    [Fact]
    public async Task HandleOption_ShouldNotApplyLimiter_WhenMethodIsNotSet()
    {
        var option = new RateLimitingOptions { Method = RateLimitingMethod.None };
        var context = new RequestContext() { Options = [option] };

        _contextServiceMock.Setup(service => service.GetRequestContext()).ReturnsAsync(context);
        await _rateLimitingService.HandleRequest();

        _fixedRequestsLimiterMock.Verify(limiter => limiter.Limit(It.IsAny<RateLimitingOptions>(), It.IsAny<RequestContext>()), Times.Never);
        _timeSinceLastRequestLimiterMock.Verify(limiter => limiter.Limit(It.IsAny<RateLimitingOptions>(), It.IsAny<RequestContext>()), Times.Never);
    }
    #endregion
}
