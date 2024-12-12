using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Tests.Rules;

public class TooCloseToLastRequestRateLimitRuleUnitTests
{
    private readonly TooCloseToLastRequestRateLimitRuleApplicator rule;

    public TooCloseToLastRequestRateLimitRuleUnitTests()
    {
        rule = new TooCloseToLastRequestRateLimitRuleApplicator();
    }

    [Fact]
    public void ApplyShouldReturnSuccessWhenThereAreNoRequestsForTheGivenKey()
    {
        // arrange
        var configuration = new RateLimitRuleConfiguration
        {
            TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
            Type = RateLimitRules.TooCloseToLastRequest
        };

        // act
        var result = rule.Apply(configuration, []);

        // assert
        Assert.Equal(RateLimitResultStatuses.Success, result.Status);
    }

    [Fact]
    public void ApplyShouldReturnSuccessWhenTheLastRequestIsFarEnoughInThePast()
    {
        // arrange
        var configuration = new RateLimitRuleConfiguration
        {
            TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
            Type = RateLimitRules.TooCloseToLastRequest
        };

        // act
        var result = rule.Apply(configuration, [DateTime.UtcNow.AddSeconds(-15)]);

        // assert
        Assert.Equal(RateLimitResultStatuses.Success, result.Status);
    }

    [Fact]
    public void ApplyShouldReturnFailureWhenTheLastRequestIsNotFarEnoughInThePast()
    {
        // arrange
        var configuration = new RateLimitRuleConfiguration
        {
            TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 10,
            Type = RateLimitRules.TooCloseToLastRequest
        };

        // act
        var result = rule.Apply(configuration, [DateTime.UtcNow.AddSeconds(-5)]);

        // assert
        Assert.Equal(RateLimitResultStatuses.Failure, result.Status);
        Assert.Equal("Please allow at least 10 seconds between requests.", result.Message);
    }
}