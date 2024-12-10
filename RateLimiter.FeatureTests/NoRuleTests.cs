using NUnit.Framework;

namespace RateLimiter.FeatureTests;

[TestFixture]
public class NoRuleTests
{
    [Test]
    public async Task Given_no_rules_then_should_grant_access()
    {
        // given

        // A client StarkIndustries who is making to call to the api
        var client = "StarkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);

        // and given the client has been given unlimited access to the api
        var rateLimiter = new RateLimiter(client);

        // when
        // Tony Stark makes a call to the api
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        // he should be allowed access
        Assert.That(result.AccessGranted, Is.True);
    }
}

