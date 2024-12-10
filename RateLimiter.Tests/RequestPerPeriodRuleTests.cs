using Moq;
using NUnit.Framework;
using RateLimiter.Domain;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RequestPerPeriodRuleTests
{
    private string _client;
    private ApiEndpoint _endpoint;
    private ITokenInspector _tokenInspector;
    
    public RequestPerPeriodRuleTests()
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
    [TestCase(0, true)]
    [TestCase(9, true)]
    [TestCase(10, false)]
    [TestCase(11, false)]
    public async Task Should_Not_Allow_If_Too_Many_Requests(int numberOfRequests, bool isWithinLimit)
    {
        // arrange
        var timeSpan = 10.Seconds();
        var clientDb = new Mock<IClientDb>();
        clientDb
            .Setup(x => x.GetRequestCount(_client, timeSpan))
            .ReturnsAsync(numberOfRequests);
        var rule = new RequestPerPeriodRule(_tokenInspector, clientDb.Object, 10, timeSpan);
        
        // act
        var result = await rule.Check("token", _endpoint);
        
        // assert
        Assert.That(result.WithInLimit, Is.EqualTo(isWithinLimit));
        
        if (!result.WithInLimit)
            Assert.That(result.Message, Is.Not.Null);
    }

    // unhappy path
    
    [Test]
    public void Should_Not_Allow_Default_Period()
    {
        var mockDb = new Mock<IClientDb>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new RequestPerPeriodRule(_tokenInspector, mockDb.Object, 1, default));
    }
}