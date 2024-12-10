using Moq;
using NUnit.Framework;
using RateLimiter.Domain;

namespace RateLimiter.FeatureTests;

[TestFixture]
public class RegionRuleTests
{
    [Test]
    public async Task Given_call_in_US_and_not_within_limits_then_should_not_grant_access()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. api calls from US can occur 10 times per sec)
        // and the client has not exhausted their limit
        var period = 1.Seconds();
        var tokenInspector = GetTokenInspector(client);
        var clientDb = GetClientDb(client, period, 10);
        var usRule = new RequestPerPeriodRule(tokenInspector.Object, clientDb.Object, 10, period);
        var rule = new RegionBaseRule(tokenInspector.Object, usRule, null);
        
        var rateLimiter = new RateLimiter(client, rule);
        
        // when
        // Peter Parker makes a call to the api
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        Assert.That(result.AccessGranted, Is.False);
    }
    
    [Test]
    public async Task Given_call_in_US_and_within_limits_then_should_grant_access()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. api calls from US can occur 10 times per sec)
        // and the client has not exhausted their limit
        var period = 1.Seconds();
        var tokenInspector = GetTokenInspector(client);
        var clientDb = GetClientDb(client, period, 1);
        var usRule = new RequestPerPeriodRule(tokenInspector.Object, clientDb.Object, 10, period);
        var rule = new RegionBaseRule(tokenInspector.Object, usRule, null);
        
        var rateLimiter = new RateLimiter(client, rule);
        
        // when
        // Peter Parker makes a call to the api
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        Assert.That(result.AccessGranted, Is.True);
    }
    
    [Test]
    public async Task Given_call_in_EU_and_call_too_soon_then_should_not_grant_access()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. api calls from US can occur 10 times per sec)
        // and the client has not exhausted their limit
        var period = 1.Seconds();
        var lastRequest = "1/1/2025 10:00:00".ToDateTime();
        var tokenInspector = GetTokenInspector(client, Region.EU);
        var clientDb = GetClientDb(client, lastRequest);
        var euRule = new TimeElapseRule(tokenInspector.Object, clientDb.Object, period);
        var rule = new RegionBaseRule(tokenInspector.Object, null, euRule);
        
        var rateLimiter = new RateLimiter(client, rule);
        
        // when
        // Peter Parker makes a call and enough time has passed
        var now = lastRequest.AddMilliseconds(50);
        var mockCurrentTime = new Mock<IClock>();
        mockCurrentTime
            .Setup(x => x.Now())
            .ReturnsAsync(now);
        euRule.Clock = mockCurrentTime.Object;
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        Assert.That(result.AccessGranted, Is.False);
    }
    
    [Test]
    public async Task Given_call_in_EU_and_within_limits_then_should_grant_access()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. api calls from US can occur 10 times per sec)
        // and the client has not exhausted their limit
        var period = 1.Seconds();
        var lastRequest = "1/1/2025 10:00:00".ToDateTime();
        var tokenInspector = GetTokenInspector(client, Region.EU);
        var clientDb = GetClientDb(client, lastRequest);
        var euRule = new TimeElapseRule(tokenInspector.Object, clientDb.Object, period);
        var rule = new RegionBaseRule(tokenInspector.Object, null, euRule);
        
        var rateLimiter = new RateLimiter(client, rule);
        
        // when
        // Peter Parker makes a call and enough time has passed
        var now = lastRequest.AddSeconds(1);
        var mockCurrentTime = new Mock<IClock>();
        mockCurrentTime
            .Setup(x => x.Now())
            .ReturnsAsync(now);
        euRule.Clock = mockCurrentTime.Object;
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        Assert.That(result.AccessGranted, Is.True);
    }
    
    private Mock<ITokenInspector> GetTokenInspector(string client, Region region = Region.US)
    {
        var tokenInspector = new Mock<ITokenInspector>();
        tokenInspector
            .Setup(x => x.GetInfo(It.IsAny<string>()))
            .ReturnsAsync(new TokenInfo {Client = client, Region = region});
        
        return tokenInspector;
    }

    private Mock<IClientDb> GetClientDb(string client, TimeSpan timeSpan, int numberOfRequests)
    {
        var clientDb = new Mock<IClientDb>();
        clientDb
            .Setup(x => x.GetRequestCount(client, timeSpan))
            .ReturnsAsync(numberOfRequests);
        
        return clientDb;
    }
    
    private Mock<IClientDb> GetClientDb(string client, DateTime lastRequest)
    {
        var clientDb = new Mock<IClientDb>();
        clientDb
            .Setup(x => x.GetLastRequestTime(client))
            .ReturnsAsync(lastRequest);
        
        return clientDb;
    }
}