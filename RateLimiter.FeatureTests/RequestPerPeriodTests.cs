using Moq;
using NUnit.Framework;
using RateLimiter.Domain;

namespace RateLimiter.FeatureTests;

[TestFixture]
public class RequestPerPeriodTests
{
    [Test]
    public async Task Given_too_many_requests_per_min_then_should_not_grant_access()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. can make 1 call every 10 secs)
        var period = 60.Seconds();
        var tokenInspector = GetTokenInspector(client);
        var clientDb = GetClientDb(client, period, 1);
        var rule = new RequestPerPeriodRule(tokenInspector.Object, clientDb.Object, 1, period);
        
        var rateLimiter = new RateLimiter(client, rule);
        
        // when 
        // Peter Paker makes too many calls to the api
        var result = await rateLimiter.CanAccess(endpoint);
        
        // then
        // he should be denied access
        Assert.That(result.AccessGranted, Is.False);
    }

    [Test]
    public async Task Given_within_call_per_min_limits_then_should_grant_access()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. can make 1 call every 10 secs)
        var period = 60.Seconds();
        var tokenInspector = GetTokenInspector(client);
        var clientDb = GetClientDb(client, period, 0);
        var rule = new RequestPerPeriodRule(tokenInspector.Object, clientDb.Object, 1, period);
        
        var rateLimiter = new RateLimiter(client, rule);
        
        // when 
        // Peter Paker makes too many calls to the api
        var result = await rateLimiter.CanAccess(endpoint);
        
        // then
        // he should be denied access
        Assert.That(result.AccessGranted, Is.True);
    }

    private Mock<ITokenInspector> GetTokenInspector(string client)
    {
        var tokenInspector = new Mock<ITokenInspector>();
        tokenInspector
            .Setup(x => x.GetInfo(It.IsAny<string>()))
            .ReturnsAsync(new TokenInfo {Client = client, Region = Region.US});
        
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
}