using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RateLimiter.Dtos;
using RateLimiter.Interfaces;
using RateLimiter.Options;
using RateLimiter.Services;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimitRuleAServiceTests
{
    private  Mock<IMemoryCacheService> _memoryCacheServiceMock;
    private Mock<IOptionsMonitor<RateLimiterOptions>> _optionsMonitorMock;
    private RateLimiterOptions _optionsMonitor;
    private  MemoryCacheEntryOptions _cacheEntryOptions;
    private IRateLimitRule _rateLimitRuleAService; 

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
                TimespanSeconds = System.TimeSpan.FromSeconds(120)
            },
            RuleB = new RuleBOptions()
            {

            }
        };
        _optionsMonitorMock = new Mock<IOptionsMonitor<RateLimiterOptions>>();
        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_optionsMonitor);
        _memoryCacheServiceMock = new Mock<IMemoryCacheService>();
        _rateLimitRuleAService = new RateLimitRuleAService(_memoryCacheServiceMock.Object, _optionsMonitorMock.Object);
        //new ConfigurationBuilder()
        //        .AddInMemoryCollection(keyValue)
        //        .AddJsonFile(_fileName, false)
        //        .Build();

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

        var result = await _rateLimitRuleAService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();


    }

    [Test]
    public async Task WhenUserHasRecord_ShouldBeCheckedForRateLimitRule_ReturnFalse()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        var record = new RuleADto()
        {
            LastCallDateTime = DateTime.UtcNow,
            RequestCount = 1,
        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleADto>(It.IsAny<string>())).Returns(record);

        //Act 

        var result = await _rateLimitRuleAService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();
    }


    [Test]
    public async Task WhenUserHasRecord_AndInTimeSpan_HasExceedRateLimit_ShouldBeRestricted_ReturnTrue()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        var record = new RuleADto()
        {
            LastCallDateTime = DateTime.UtcNow.AddSeconds(-60),
            RequestCount = 6,
        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleADto>(It.IsAny<string>())).Returns(record);

        //Act 

        var result = await _rateLimitRuleAService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeTrue();
    }

    [Test]
    public async Task WhenUserHasRecord_AndInTimeSpan_HasNotExceedRateLimit_ShouldNotBeRestricted_ReturnFalse()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        var record = new RuleADto()
        {
            LastCallDateTime = DateTime.UtcNow.AddSeconds(-60),
            RequestCount = 4,
        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleADto>(It.IsAny<string>())).Returns(record);

        //Act 

        var result = await _rateLimitRuleAService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();
    }


    [Test]
    public async Task WhenUserHasRecord_AndInTimeSpanHAsPassed_OldRecordShouldBeRemovedFromMemory_ReturnFalse()
    {
        //Arrange

        RateLimitRuleRequestDto userInfo = new()
        {
            UserId = 1,
            UserLocal = "EU"
        };

        var record = new RuleADto()
        {
            LastCallDateTime = DateTime.UtcNow.AddSeconds(-400),
            RequestCount = 4,
        };

        _memoryCacheServiceMock.Setup(x => x.Get<RuleADto>(It.IsAny<string>())).Returns(record);

        //Act 

        var result = await _rateLimitRuleAService.IsRequestAllowed(userInfo);

        //Assert

        result.Should().BeFalse();
    }



}
