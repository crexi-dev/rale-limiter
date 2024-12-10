using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiter.Enums;
using RateLimiter.Middleware;
using RateLimiter.Models;
using RateLimiter.Persistence;
using RateLimiter.RuleApplicators;
using System.Net;

namespace RateLimiter.Tests.Middleware;

public class CustomRateLimiterMiddlewareUnitTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHost _host;
    private readonly Mock<IProvideAccessToCachedData> _moqRepository;
    private readonly Mock<IApplyARateLimit> _moqTooCloseToLastRequestRateLimitRule;
    private readonly Mock<IApplyARateLimit> _moqTooManyInTimeSpanRateLimitRule;

    public CustomRateLimiterMiddlewareUnitTests()
    {
        _moqRepository = new Mock<IProvideAccessToCachedData>();
        _moqTooCloseToLastRequestRateLimitRule = new Mock<IApplyARateLimit>();
        _moqTooManyInTimeSpanRateLimitRule = new Mock<IApplyARateLimit>();

        _moqRepository.Setup(p => p.GetRequestsByKey(It.IsAny<string>())).Returns([new DateTime(2024, 12, 12, 03, 04, 05), new DateTime(2024, 12, 12, 03, 04, 06)]);

        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns(
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
                    Type = RateLimitRules.TooCloseToLastRequest
                }
            ]
        );

        _moqTooCloseToLastRequestRateLimitRule.SetupGet(p => p.Type).Returns(RateLimitRules.TooCloseToLastRequest);
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult());

        _moqTooManyInTimeSpanRateLimitRule.SetupGet(p => p.Type).Returns(RateLimitRules.TooManyInTimeSpan);
        _moqTooManyInTimeSpanRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult());

        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(_moqTooManyInTimeSpanRateLimitRule.Object);
                        services.AddSingleton(_moqTooCloseToLastRequestRateLimitRule.Object);

                        services.AddSingleton(_moqRepository.Object);
                    })
                    .Configure(app => app.UseCustomRateLimiter());
            })
            .Start();

        _client = _host.GetTestClient();
        _client.DefaultRequestHeaders.Add("x-authentication-key", "test-token");
    }

    public void Dispose()
    {
        _client.Dispose();
        _host.Dispose();
    }

    [Fact]
    public async void MiddlewareShouldReturnAnUnauthorizedResponseWhenThereIsNoTokenHeader()
    {
        // arrange
        using var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => services.AddCustomRateLimiterRules())
                    .Configure(app => app.UseCustomRateLimiter());
            })
            .Start();

        var client = host.GetTestClient();

        // act
        var response = await client.GetAsync("/test-endpoint");

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async void MiddlewareShouldGetTheRulesForTheGivenTokenAndResource()
    {
        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.GetRuleConfigurationsByKey("test-token_/test-endpoint"), Times.Once);
    }

    [Fact]
    public async void MiddlewareShouldLockTheRequestsForTheGivenTokenAndResource()
    {
        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.Lock("test-token_/test-endpoint"), Times.Once);
    }

    #region No Rules

    [Fact]
    public async void MiddlewareShouldAddTheCurrentRequestToTheListOfRequestsWhenThereAreNoRulesToApply()
    {
        // arrange
        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns([]);

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.AddRequestByKey("test-token_/test-endpoint"), Times.Once);
    }

    [Fact]
    public async void MiddlewareShouldUnlockTheRequestsForTheGivenTokenAndResourceWhenThereAreNoRulesToApply()
    {
        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.Unlock("test-token_/test-endpoint"), Times.Once);
    }

    [Fact]
    public async void MiddlewareShouldContinueTheRequestWhenThereAreNoRulesToApply()
    {
        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        // Since we didn't configure an endpoint, the NotFound response indicates the request continued without interruption
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    [Fact]
    public async void MiddlewareShouldApplyTheRulesThatMatchTheRuleTypesForTheGivenTokenAndResource()
    {
        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqTooCloseToLastRequestRateLimitRule.Verify(p =>
            p.Apply(
                It.Is<RateLimitRuleConfiguration>(rc => rc.Type == RateLimitRules.TooCloseToLastRequest && rc.TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds == 10),
                new List<DateTime> { new DateTime(2024, 12, 12, 03, 04, 05), new DateTime(2024, 12, 12, 03, 04, 06) }),
            Times.Once);
        _moqTooManyInTimeSpanRateLimitRule.Verify(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>()), Times.Never);
    }

    #region One Rule

    [Fact]
    public async void MiddlewareShouldContinueTheRequestWhenThereIsOnlyOneRuleToApplyAndItThrowsAnException()
    {
        // arrange
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Throws(new Exception("something went wrong"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async void MiddlewareShouldReturnATooManyRequestsResponseWhenThereIsOnlyOneRuleToApplyAndItFails()
    {
        // arrange
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "Some message indicating the problem"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        // A response with status code 429 means the middleware stopped the request properly
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"errorMessage\":\"Some message indicating the problem\"}", content);
    }

    [Fact]
    public async void MiddlewareShouldNotAddTheCurrentRequestToTheListOfRequestsWhenThereIsOnlyOneRuleToApplyAndItFails()
    {
        // arrange
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "Some message indicating the problem"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.AddRequestByKey(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void MiddlewareShouldUnlockTheRequestsForTheGivenTokenAndResourceWhenThereIsOnlyOneRuleToApplyAndItFails()
    {
        // arrange
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "Some message indicating the problem"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.Unlock("test-token_/test-endpoint"), Times.Once);
    }

    #endregion

    #region Multiple Rules, One Throws an Exception

    [Fact]
    public async void MiddlewareShouldReturnATooManyRequestsResponseWhenThereAreMultipleRulesToApplyAndOneFailsWhileAnotherThrowsAnException()
    {
        // arrange
        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns(
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
                    Type = RateLimitRules.TooCloseToLastRequest
                },
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                }
            ]
        );
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Throws(new Exception("something went wrong"));
        _moqTooManyInTimeSpanRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "A failure message"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"errorMessage\":\"A failure message\"}", content);
    }

    [Fact]
    public async void MiddlewareShouldNotAddTheCurrentRequestToTheListOfRequestsWhenThereAreMultipleRulesToApplyAndOneFailsWhileAnotherThrowsAnException()
    {
        // arrange
        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns(
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
                    Type = RateLimitRules.TooCloseToLastRequest
                },
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                }
            ]
        );
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Throws(new Exception("something went wrong"));
        _moqTooManyInTimeSpanRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "A failure message"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.AddRequestByKey(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void MiddlewareShouldUnlockTheRequestsForTheGivenTokenAndResourceWhenThereAreMultipleRulesToApplyAndOneFailsWhileAnotherThrowsAnException()
    {
        // arrange
        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns(
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
                    Type = RateLimitRules.TooCloseToLastRequest
                },
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                }
            ]
        );
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Throws(new Exception("something went wrong"));
        _moqTooManyInTimeSpanRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "A failure message"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqRepository.Verify(p => p.Unlock("test-token_/test-endpoint"), Times.Once);
    }

    #endregion

    #region Multiple Rules

    [Fact]
    public async void MiddlewareShouldReturnATooManyRequestsResponseWhenThereAreMultipleRulesToApplyAndTheFirstOneFails()
    {
        // arrange
        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns(
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
                    Type = RateLimitRules.TooCloseToLastRequest
                },
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                }
            ]
        );
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "This should be the first rule checked"));
        _moqTooManyInTimeSpanRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "A failure message"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"errorMessage\":\"This should be the first rule checked\"}", content);
    }

    [Fact]
    public async void MiddlewareShouldNotApplySubsequentRulesWhenThereAreMultipleRulesToApplyAndTheFirstOneFails()
    {
        // arrange
        _moqRepository.Setup(p => p.GetRuleConfigurationsByKey(It.IsAny<string>())).Returns(
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
                    Type = RateLimitRules.TooCloseToLastRequest
                },
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                }
            ]
        );
        _moqTooCloseToLastRequestRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "This should be the first rule checked"));
        _moqTooManyInTimeSpanRateLimitRule.Setup(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>())).Returns(new RateLimitResult(RateLimitResultStatuses.Failure, "A failure message"));

        // act
        var response = await _client.GetAsync("/test-endpoint");

        // assert
        _moqTooManyInTimeSpanRateLimitRule.Verify(p => p.Apply(It.IsAny<RateLimitRuleConfiguration>(), It.IsAny<List<DateTime>>()), Times.Never);
    }

    #endregion
}