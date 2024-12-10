using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RateLimiter.Domain;

namespace RateLimiter.Tests;

[TestFixture]
public class RegionBaseRuleTests
{
    private string _client;
    private ApiEndpoint _endpoint;

    public RegionBaseRuleTests()
    {
        _client = "TestClient";
        _endpoint = new ApiEndpoint("http://www.example.com/api/example");
    }
    
    // happy path
    
    [Test]
    public async Task Should_Check_US_Rule()
    {
        // arrange
        var usRule = new Mock<IRateRule>();
        var euRule = new Mock<IRateRule>();
        var tokenInspector = new Mock<ITokenInspector>();
        tokenInspector
            .Setup(x => x.GetInfo(It.IsAny<string>()))
            .ReturnsAsync(new TokenInfo { Client = _client, Region = Region.US });
        
        var regionBaseRule = new RegionBaseRule(tokenInspector.Object, usRule.Object, euRule.Object);
        
        // act
        _ = await regionBaseRule.Check("token", _endpoint);

        // assert
        usRule.Verify(x => x.Check("token", _endpoint), Times.Once, "US rule should be checked");
        euRule.Verify(x => x.Check("token", _endpoint), Times.Never, "EU rule should not be checked");
    }
    
    [Test]
    public async Task Should_Check_EU_Rule()
    {
        // arrange
        var usRule = new Mock<IRateRule>();
        var euRule = new Mock<IRateRule>();
        var tokenInspector = new Mock<ITokenInspector>();
        tokenInspector
            .Setup(x => x.GetInfo(It.IsAny<string>()))
            .ReturnsAsync(new TokenInfo { Client = _client, Region = Region.EU });
        
        var regionBaseRule = new RegionBaseRule(tokenInspector.Object, usRule.Object, euRule.Object);
        
        // act
        _ = await regionBaseRule.Check("token", _endpoint);

        // assert
        usRule.Verify(x => x.Check("token", _endpoint), Times.Never);
        euRule.Verify(x => x.Check("token", _endpoint), Times.Once);
    }
    
    // unhappy path
}