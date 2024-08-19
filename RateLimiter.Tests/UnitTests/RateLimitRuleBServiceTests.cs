using RateLimiter.Dtos;
using RateLimiter.Interfaces;
using RateLimiter.Options;
using RateLimiter.Services;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimitRuleBServiceTests
{
    private Mock<IMemoryCacheService> _memoryCacheServiceMock;
    private Mock<IOptionsMonitor<RateLimiterOptions>> _optionsMonitorMock;
    private RateLimiterOptions _optionsMonitor;
    private MemoryCacheEntryOptions _cacheEntryOptions;
    private IRateLimitRule _rateLimitRuleBService;

    [SetUp]
    public void SetUp()
    {
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };
        _optionsMonitor = new RateLimiterOptions()
        {
            RuleA = new RuleAOptions()
            {
                RequestsPerTimespan = 5,
                TimespanSeconds = TimeSpan.FromSeconds(120)
            },
            RuleB = new RuleBOptions()
            {
                MinTimespanBetweenCallsSeconds = TimeSpan.FromSeconds(120)
            }
        };
        _optionsMonitorMock = new Mock<IOptionsMonitor<RateLimiterOptions>>();
        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_optionsMonitor);
        _memoryCacheServiceMock = new Mock<IMemoryCacheService>();
        _rateLimitRuleBService = new RateLimitRuleBService(_memoryCacheServiceMock.Object, _optionsMonitorMock.Object);


    }

    [Test]

    public async Task WhenUserDoesNotHaveAnyRecord_CreateAnewRecordInmemory()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleADto>(It.IsAny<string>())).Returns((RuleADto)null);

        //Act 

        var result = await _rateLimitRuleBService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();


    }

    [Test]
    public async Task WhenUserHasRecord_CertainTimeHasPassed_ReturnTrue()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        var record = new RuleBDto()
        {
            LastCallDateTime = DateTime.UtcNow

        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleBDto>(It.IsAny<string>())).Returns(record);

        //Act 

        var result = await _rateLimitRuleBService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeTrue();
    }


    [Test]
    public async Task WhenUserHasRecord_CertainTimeHasNotPassed_ReturnFalse()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        var record = new RuleBDto()
        {
            LastCallDateTime = DateTime.UtcNow.AddSeconds(-300)

        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleBDto>(It.IsAny<string>())).Returns(record);

        //Act 

        var result = await _rateLimitRuleBService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();
    }

}
