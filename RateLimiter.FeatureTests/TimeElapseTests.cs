using Moq;
using NUnit.Framework;
using RateLimiter.Domain;

namespace RateLimiter.FeatureTests;

[TestFixture]
public class TimeElapseTests
{
    [Test]
    public async Task Given_calling_api_too_soon_then_should_not_be_allowed()
    {
        // given
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. calls has to be at least 1 sec apart)
        var lastRequest = "1/1/2025 10:00:00".ToDateTime();
        var tokenInspector = GetTokenInspector(client);
        var clientDb = GetClientDb(client, lastRequest);
        var rule = new TimeElapseRule(tokenInspector.Object, clientDb.Object, 1.Seconds());

        var rateLimiter = new RateLimiter(client, rule);

        // when
        // Peter Parker makes a call too soon
        var now = lastRequest.AddMilliseconds(50);
        var mockCurrentTime = new Mock<IClock>();
        mockCurrentTime
            .Setup(x => x.Now())
            .ReturnsAsync(now);
        rule.Clock = mockCurrentTime.Object;
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        // The call should not be allowed
        Assert.That(result.AccessGranted, Is.False);
    }
    
    [Test]
    public async Task Given_calling_api_past_period_then_should_be_allowed()
    {
        // give
        // A client ParkerIndustries who is making to call to the api
        var client = "ParkIndustries";
        var endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);
        
        // and given the client has limited access (i.e. calls has to be at least 1 sec apart)
        var lastRequest = "1/1/2025 10:00:00".ToDateTime();
        var tokenInspector = GetTokenInspector(client);
        var clientDb = GetClientDb(client, lastRequest);
        var rule = new TimeElapseRule(tokenInspector.Object, clientDb.Object, 1.Seconds());

        var rateLimiter = new RateLimiter(client, rule);

        // when
        // Peter Parker makes a call too soon
        var now = lastRequest.AddSeconds(1);
        var mockCurrentTime = new Mock<IClock>();
        mockCurrentTime
            .Setup(x => x.Now())
            .ReturnsAsync(now);
        rule.Clock = mockCurrentTime.Object;
        var result = await rateLimiter.CanAccess(endpoint);

        // then
        // The call should not be allowed
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
    
    private Mock<IClientDb> GetClientDb(string client, DateTime lastRequest)
    {
        var clientDb = new Mock<IClientDb>();
        clientDb
            .Setup(x => x.GetLastRequestTime(client))
            .ReturnsAsync(lastRequest);
        
        return clientDb;
    }
}