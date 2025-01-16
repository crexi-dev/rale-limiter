using System.Threading.Tasks;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class InMemoryRateLimiterRuleStorageTests
{
    private IRateLimiterRuleStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = new InMemoryRateLimiterRuleStorage();
    }

    [Test]
    public async Task Should_Be_Able_To_Store_And_Get_Rules()
    {
        // Act
        await _storage.AddOrUpdateRuleAsync(RateLimiterRules.TeamBApiRuleUsa);
        await _storage.AddOrUpdateRuleAsync(RateLimiterRules.TeamBApiRuleEurope);
        await _storage.AddOrUpdateRuleAsync(RateLimiterRules.MarketingPhoneNumberRule);

        // Assert
        var teamBUsaRule = await _storage.GetRuleAsync("team-b", new RateLimitDescriptor("location", "us"));
        var teamBEuropeRule = await _storage.GetRuleAsync("team-b", new RateLimitDescriptor("location", "eu"));
        var marketingRule1 = await _storage.GetRuleAsync("marketing", new RateLimitDescriptor("phone", "555-5678"));
        var marketingRule2 = await _storage.GetRuleAsync("marketing", new RateLimitDescriptor("phone", "555-1234"));

        Assert.AreEqual(RateLimiterRules.TeamBApiRuleUsa, teamBUsaRule);
        Assert.AreEqual(RateLimiterRules.TeamBApiRuleEurope, teamBEuropeRule);
        Assert.AreEqual(RateLimiterRules.MarketingPhoneNumberRule, marketingRule1);
        Assert.AreEqual(RateLimiterRules.MarketingPhoneNumberRule, marketingRule2);

    }

    [Test]
    public async Task Should_Return_Null_When_Rule_Not_Found()
    {
        // Act
        var rule = await _storage.GetRuleAsync("team-b", new RateLimitDescriptor("location", "us"));

        // Assert
        Assert.Null(rule);
    }

}
