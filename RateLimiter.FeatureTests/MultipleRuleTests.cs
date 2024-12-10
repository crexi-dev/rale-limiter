using Moq;
using NUnit.Framework;
using RateLimiter.Domain;

namespace RateLimiter.FeatureTests;

[TestFixture]
public class MultipleRuleTests
{
    Mock<ITokenInspector> _tokenInspector;
    string _client;
    ApiEndpoint _endpoint;
    private Mock<IClientDb> _clientDb;
    private DateTime _lastRequest;
    private TimeSpan _period;
    
    [SetUp]
    public void Given()
    {
        // A client ParkerIndustries who is making to call to the api
        _client = "ParkIndustries";
        _endpoint = new ApiEndpoint("www.starkindustries.com/api/heros", HttpMethod.Get);

        _tokenInspector = CreateTokenInspector(_client);
        
        _lastRequest = "1/1/2025 10:00:00".ToDateTime();
        _clientDb = CreateClientDb(_client, _lastRequest, 5);
        
        _period = 60.Seconds();
    }

    [Test] 
    public async Task Given_within_limits_when_should_be_granted_access()
    {
        // given (see setup)
        
        // and given the client has limited access
        // (i.e. calls has to be at least 1 sec apart and can make 10 call every 1 min)
        var rule1 = CreateTimeElapseRule(1.Seconds());
        var rule2 = CreateRequestPerPeroidRule(10, _period);
        
        var rateLimiter = new RateLimiter(_client, rule1, rule2);
        
        // when
        // Peter Paker makes calls within limits
        rule1.SetCurrentTime(_lastRequest.AddSeconds(2));
        var result = await rateLimiter.CanAccess(_endpoint);

        // then 
        Assert.That(result.AccessGranted, Is.True);
    }
    
    [Test] 
    public async Task Given_not_within_limits_when_should_not_be_granted_access()
    {
        // given (see setup)
        
        // and given the client has limited access
        // (i.e. calls has to be at least 1 sec apart and can make 10 call every 1 min)
        var rule1 = CreateTimeElapseRule(1.Seconds());  // should succeed
        var rule2 = CreateRequestPerPeroidRule(2, _period);   // should fail
        
        var rateLimiter = new RateLimiter(_client, rule1, rule2);
        
        // when
        // Peter Paker makes calls within limits
        rule1.SetCurrentTime(_lastRequest.AddSeconds(2));
        var result = await rateLimiter.CanAccess(_endpoint);

        // then 
        Assert.That(result.AccessGranted, Is.True);
    }
    
    private Mock<ITokenInspector> CreateTokenInspector(string client)
    {
        var tokenInspector = new Mock<ITokenInspector>();
        tokenInspector
            .Setup(x => x.GetInfo(It.IsAny<string>()))
            .ReturnsAsync(new TokenInfo {Client = client, Region = Region.US});
        
        return tokenInspector;
    }

    private Mock<IClientDb> CreateClientDb(string client, DateTime lastRequest, int periodCount)
    {
        var clientDb = new Mock<IClientDb>();
        clientDb
            .Setup(x => x.GetLastRequestTime(client))
            .ReturnsAsync(lastRequest);
        
        clientDb
            .Setup(x => x.GetRequestCount(client, _period))
            .ReturnsAsync(periodCount);
        
        return clientDb;
    }

    private TimeElapseRule CreateTimeElapseRule(TimeSpan period)
    {
        return new TimeElapseRule(_tokenInspector.Object, _clientDb.Object, period);
    }
    
    private RequestPerPeriodRule CreateRequestPerPeroidRule(int maxRequest, TimeSpan period)
    {
        return new RequestPerPeriodRule(_tokenInspector.Object, _clientDb.Object, maxRequest, period);
    }
}

public static class Extensions
{
    public static void SetCurrentTime(this TimeElapseRule timeElapseRule, DateTime now)
    {
        var mockCurrentTime = new Mock<IClock>();
        mockCurrentTime
            .Setup(x => x.Now())
            .ReturnsAsync(now);
        timeElapseRule.Clock = mockCurrentTime.Object;
    }
}

