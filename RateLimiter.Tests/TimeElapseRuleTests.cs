using Moq;
using NUnit.Framework;
using RateLimiter.Domain;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class TimeElapseRuleTests
{
    private string _client;
    private ApiEndpoint _endpoint;
    private ITokenInspector _tokenInspector;
    
    public TimeElapseRuleTests()
    {
        _client = "TestClient";
        _endpoint = new ApiEndpoint("http://www.example.com/api/example");
        
        var mockTokenInspector = new Mock<ITokenInspector>();
        mockTokenInspector
            .Setup(x => x.GetInfo(It.IsAny<string>()))
            .ReturnsAsync(new TokenInfo {Client = _client, Region = Region.US});
        _tokenInspector = mockTokenInspector.Object;
    }
    
    // happy path

    [TestCase(10)]
    [TestCase(11)]
    public async Task Should_Allow_If_Enough_Time_Pasted(int seconds)
    {
        // arrange
        var lastRequest = "1/1/2025 10:00:00".ToDateTime();
        var nextRequest = lastRequest.AddSeconds(seconds);
        
        var mockDb = new Mock<IClientDb>();
        mockDb
            .Setup(x => x.GetLastRequestTime(_client))
            .ReturnsAsync(lastRequest);
        
        var mockTime = new Mock<IClock>();
        mockTime
            .Setup(x => x.Now())
            .ReturnsAsync(nextRequest);
        
        var timeSpanRule = new TimeElapseRule(_tokenInspector, mockDb.Object, 10.Seconds())
        {
            Clock = mockTime.Object
        };
        
        // act
        var result = await timeSpanRule.Check("token", _endpoint);

        // assert
        Assert.That(result.WithInLimit, Is.True, "Should return true");
        Assert.That(result.Message, Is.Empty, "Should not return a message");
    }
    
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(9)]
    public async Task Should_Not_Allow_If_Not_Enough_Time_Pasted(int seconds)
    {
        // arrange
        var lastRequest = "1/1/2025 10:00:00".ToDateTime();
        var nextRequest = lastRequest.AddSeconds(seconds);
        
        var mockDb = new Mock<IClientDb>();
        mockDb
            .Setup(x => x.GetLastRequestTime(_client))
            .ReturnsAsync(lastRequest);
        
        var mockTime = new Mock<IClock>();
        mockTime
            .Setup(x => x.Now())
            .ReturnsAsync(nextRequest);
        
        var timeSpanRule = new TimeElapseRule(_tokenInspector, mockDb.Object, 10.Seconds())
        {
            Clock = mockTime.Object
        };
        
        // act
        var result = await timeSpanRule.Check("token", _endpoint);

        // assert
        Assert.That(result.WithInLimit, Is.False, "Should return true");
        Assert.That(result.Message, Is.Not.Empty, "Should return a message");
    }
}