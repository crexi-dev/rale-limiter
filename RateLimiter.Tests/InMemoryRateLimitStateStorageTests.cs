using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class InMemoryRateLimitStateStorageTests
{
    private IRateLimitStateStorage<int> _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = new InMemoryRateLimitStateStorage();    
    }

    [Test]
    public async Task Should_Be_Able_To_Store_And_Get_State()
    {
        var teamBUsaRuleKey = RateLimiterRules.TeamBApiRuleUsa.GetHashCode();
        var expectedTeamBUsaRuleState = new RateLimitRuleState(10, DateTime.UtcNow.AddSeconds(-10));

        var teamBEuropeRuleKey = RateLimiterRules.TeamBApiRuleEurope.GetHashCode();
        var expectedTeamBEuropeRuleState = new RateLimitRuleState(1, DateTime.UtcNow.AddSeconds(-5));

        var marketingRuleKey = RateLimiterRules.MarketingPhoneNumberRule.GetHashCode();
        var expectedMarketingRuleState = new RateLimitRuleState(0, DateTime.UtcNow.AddSeconds(60));

        await _storage.AddOrUpdateStateAsync(teamBUsaRuleKey, expectedTeamBUsaRuleState);
        await _storage.AddOrUpdateStateAsync(teamBEuropeRuleKey, expectedTeamBEuropeRuleState);
        await _storage.AddOrUpdateStateAsync(marketingRuleKey, expectedMarketingRuleState);

        var actualTeamBUsaRuleState = await _storage.GetStateAsync(teamBUsaRuleKey);
        var actualTeamBEuropeRuleState = await _storage.GetStateAsync(teamBEuropeRuleKey);
        var actualMarketingRuleState = await _storage.GetStateAsync(marketingRuleKey);

        Assert.AreEqual(expectedTeamBUsaRuleState, actualTeamBUsaRuleState);
        Assert.AreEqual(expectedTeamBEuropeRuleState, actualTeamBEuropeRuleState);
        Assert.AreEqual(expectedMarketingRuleState, actualMarketingRuleState);
    }

}
