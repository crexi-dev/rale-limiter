using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private string _client;
    private ApiEndpoint _apiEndpoint;
    public RateLimiterTest()
    {
        _client = "TestClient";
        _apiEndpoint = new ApiEndpoint("http://www.example.com/api/example");
    }
    
    // Happy Path

    [Test]
    public async Task Should_Check_All_Rules()
    {
        // arrange
        var rule1 = new Mock<IRateRule>();
        rule1
            .Setup( x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()))
            .ReturnsAsync(new CheckStatus(true));
        
        var rule2 = new Mock<IRateRule>();
        rule2
            .Setup( x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()))
            .ReturnsAsync(new CheckStatus(true));
        
        var rule3 = new Mock<IRateRule>();
        rule3
            .Setup( x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()))
            .ReturnsAsync(new CheckStatus(true));
        
        var rateLimiter = new RateLimiter(_client, rule1.Object, rule2.Object, rule3.Object);

        // act
        var result = await rateLimiter.CanAccess(_apiEndpoint);

        // assert
        rule1.Verify(x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()), Times.Once());
        rule2.Verify(x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()), Times.Once());
        rule3.Verify(x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()), Times.Once());
        Assert.That(result.AccessGranted, Is.True);
    }

    [Test]
    public async Task Should_Allow_If_No_Rules()
    {
        // arrange
        var rateLimiter = new RateLimiter(_client);

        // act
        var result = await rateLimiter.CanAccess(_apiEndpoint);

        // assert
        Assert.That(result.AccessGranted, Is.True);
    }

    [Test]
    public async Task Should_DenyAccess_If_Not_Allowed()
    {
        // arrange
        var failedRule = new Mock<IRateRule>();
        failedRule
            .Setup( x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()))
            .ReturnsAsync(new CheckStatus(false, "denied"));
        
        var successfulRule1 = new Mock<IRateRule>();
        successfulRule1
            .Setup( x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()))
            .ReturnsAsync(new CheckStatus(true));
        
        var successfulRule2 = new Mock<IRateRule>();
        successfulRule2
            .Setup( x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()))
            .ReturnsAsync(new CheckStatus(true));
        
        var rules = new[] { successfulRule1.Object, successfulRule2.Object, failedRule.Object };
        var rateLimiter = new RateLimiter(_client, rules);
        
        // act
        var result = await rateLimiter.CanAccess(_apiEndpoint);
        
        // assert
        Assert.That(result.AccessGranted, Is.False);
        failedRule.Verify(x => x.Check(It.IsAny<string>(), It.IsAny<ApiEndpoint>()), Times.Once);
    }

    // Unhappy Path
    
    [Test]
    public async Task Should_Not_Allow_NullOrEmpty_Client()
    {
        Assert.Throws<ArgumentException>(() => new RateLimiter(null));
        Assert.Throws<ArgumentException>(() => new RateLimiter(string.Empty));
    }
}