using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Tests.Rules;

public class TooManyInTimeSpanRateLimitRuleUnitTests
{    private readonly TooManyInTimeSpanRateLimitRuleApplicator rule;

    public TooManyInTimeSpanRateLimitRuleUnitTests()
    {
        rule = new TooManyInTimeSpanRateLimitRuleApplicator();
    }

    [Fact]
    public void ApplyShouldReturnSuccessWhenThereAreNoRequestsForTheGivenKey()
    {
        // arrange
        var configuration = new RateLimitRuleConfiguration
        {
            TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
            TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
            Type = RateLimitRules.TooManyInTimeSpan
        };

        // act
        var result = rule.Apply(configuration, []);

        // assert
        Assert.Equal(RateLimitResultStatuses.Success, result.Status);
    }

    [Fact]
    public void ApplyShouldReturnSuccessWhenThereAreFewerThanTheAllowedNumberOfRequestsMadeInTheConfiguredTimeFrame()
    {
        // arrange
        var configuration = new RateLimitRuleConfiguration
        {
            TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 3,
            TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
            Type = RateLimitRules.TooManyInTimeSpan
        };

        // act
        var result = rule.Apply(configuration, [DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(-2)]);

        // assert
        Assert.Equal(RateLimitResultStatuses.Success, result.Status);
    }

    [Fact]
    public void ApplyShouldReturnFailureWhenThereAreMoreThanTwoRequestsInTheLastMinute()
    {
        // arrange
        var configuration = new RateLimitRuleConfiguration
        {
            TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 3,
            TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 60,
            Type = RateLimitRules.TooManyInTimeSpan
        };

        // act
        var result = rule.Apply(configuration, [DateTime.UtcNow.AddSeconds(-30), DateTime.UtcNow.AddSeconds(-15), DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddSeconds(-5)]);

        // assert
        Assert.Equal(RateLimitResultStatuses.Failure, result.Status);
        Assert.Equal("Only 3 requests may be made within 60 seconds.", result.Message);
    }
}