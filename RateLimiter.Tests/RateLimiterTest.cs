using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

    [Test]
    public async Task IsRequestAllowedAsync_WhenAllValidationPassed_SholdReturnTrue()
    {
        var rule1Mock = new Mock<IRateLimiterRule>();
        rule1Mock.Setup(x => x.IsRequestAllowedAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var rule2Mock = new Mock<IRateLimiterRule>();
        rule2Mock.Setup(x => x.IsRequestAllowedAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var rule3Mock = new Mock<IRateLimiterRule>();
        rule3Mock.Setup(x => x.IsRequestAllowedAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        

        var result = await RateLimiter.IsRequestAllowedAsync(fixture.Create<HttpContext>(), [rule1Mock.Object, rule2Mock.Object, rule3Mock.Object]);


        Assert.True(result);

    }

    [Test]
    public async Task IsRequestAllowedAsync_WhenSomeValidationFailed_SholdReturnTrue()
    {
        var rule1Mock = new Mock<IRateLimiterRule>();
        rule1Mock.Setup(x => x.IsRequestAllowedAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var rule2Mock = new Mock<IRateLimiterRule>();
        rule2Mock.Setup(x => x.IsRequestAllowedAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var rule3Mock = new Mock<IRateLimiterRule>();
        rule3Mock.Setup(x => x.IsRequestAllowedAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        

        var result = await RateLimiter.IsRequestAllowedAsync(fixture.Create<HttpContext>(), [rule1Mock.Object, rule2Mock.Object, rule3Mock.Object]);


        Assert.False(result);
    }

}