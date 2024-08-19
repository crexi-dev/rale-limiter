using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;
using RateLimiter.Services;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimitRuleCServiceTests
{
    private Mock<IMemoryCacheService> _memoryCacheServiceMock;
    private Mock<IOptionsMonitor<RateLimiterOptions>> _optionsMonitorMock;
    private Mock<Func<RateLimitRules, IRateLimitRule>> _funcMock;
    private RateLimiterOptions _optionsMonitor;
    private MemoryCacheEntryOptions _cacheEntryOptions;
    private IRateLimitRule _rateLimitRuleCService;
    private Mock<IRateLimitRule> _rateLimitRuleAServiceMock;
    private Mock<IRateLimitRule> _rateLimitRuleBServiceMock;


    [SetUp]
    public void SetUp()
    {
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };

        _memoryCacheServiceMock = new Mock<IMemoryCacheService>();

        _optionsMonitor = new RateLimiterOptions()
        {
            RuleA = new RuleAOptions()
            {
                RequestsPerTimespan = 5,
                TimespanSeconds = System.TimeSpan.FromSeconds(120)
            },
            RuleB = new RuleBOptions()
            {

            }
        };
        _optionsMonitorMock = new Mock<IOptionsMonitor<RateLimiterOptions>>();
        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_optionsMonitor);
        _funcMock = new Mock<Func<RateLimitRules, IRateLimitRule>>();
        _rateLimitRuleCService = new RateLimitRuleCService(_funcMock.Object);
        _rateLimitRuleAServiceMock = new Mock<IRateLimitRule>();
        _rateLimitRuleBServiceMock = new Mock<IRateLimitRule>();


    }

    [Test]

    public async Task GivenUserInUS_WhenCheckingRateLimitRuleANotPassed_ShouldReturnFalseForRateLimitRuleC()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "US"
        };

        _funcMock.Setup(f => f(RateLimitRules.RuleA)).Returns(_rateLimitRuleAServiceMock.Object);
        _rateLimitRuleAServiceMock.Setup(x => x.IsRequestAllowed(It.IsAny<RateLimitRuleRequestDto>())).ReturnsAsync(false);
        //Act 

        var result = await _rateLimitRuleCService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();


    }


    [Test]

    public async Task GivenUserInUS_WhenCheckingRateLimitRuleAPassed_ShouldReturnTrueForRateLimitRuleC()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "US"
        };


        _funcMock.Setup(f => f(RateLimitRules.RuleA)).Returns(_rateLimitRuleAServiceMock.Object);
        _rateLimitRuleAServiceMock.Setup(x => x.IsRequestAllowed(It.IsAny<RateLimitRuleRequestDto>())).ReturnsAsync(true);

        //Act 

        var result = await _rateLimitRuleCService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeTrue();


    }

    [Test]

    public async Task GivenUserInUS_WhenCheckingRateLimitRuleBNotPassed_ShouldReturnFalseForRateLimitRuleC()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        _funcMock.Setup(f => f(RateLimitRules.RuleB)).Returns(_rateLimitRuleBServiceMock.Object);
        _rateLimitRuleBServiceMock.Setup(x => x.IsRequestAllowed(It.IsAny<RateLimitRuleRequestDto>())).ReturnsAsync(false);
        //Act 

        var result = await _rateLimitRuleCService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();


    }


    [Test]

    public async Task GivenUserInUS_WhenCheckingRateLimitRuleBPassed_ShouldReturnTrueForRateLimitRuleC()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };


        _funcMock.Setup(f => f(RateLimitRules.RuleB)).Returns(_rateLimitRuleBServiceMock.Object);
        _rateLimitRuleBServiceMock.Setup(x => x.IsRequestAllowed(It.IsAny<RateLimitRuleRequestDto>())).ReturnsAsync(true);

        //Act 

        var result = await _rateLimitRuleCService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeTrue();


    }


}
