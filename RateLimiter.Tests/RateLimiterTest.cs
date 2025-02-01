using System;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using RateLimiter.Model;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private IRateLimiterFactory _rateLimiterFactory;
    private Mock<IMemoryCache> _memoryCacheMock;

    [SetUp]  
    public void SetUp()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _rateLimiterFactory = new RateLimiterFactory(_memoryCacheMock.Object);
    }

    [Test]
    public void RateLimitResource2_DoNot_Allowed()
    {
        var clientData = new ClientModel()
        {
            ClientId = "123"
        };

        var cacheKey = $"rateLimiter-lastCall-resource2-{clientData.ClientId}";
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

        object outValue = oneMinuteAgo;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(true);

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>);

        var rateLimiterResource2 = _rateLimiterFactory.CreateRateLimiter("resource2", clientData);
        var isAllowedRequest = rateLimiterResource2.IsRequestAllowed();

        Assert.That(isAllowedRequest, Is.False);
    }

    [TestCase(2, true)]
    [TestCase(5, false)]
    public void RateLimitResource1_Allowed(int requestCount, bool result)
    {
        var clientData = new ClientModel()
        {
            ClientId = "123"
        };

        var cacheKey = $"rateLimiter-count-resource1-123";

        object outValue = requestCount;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(true);

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>);

        var rateLimiterResource3 = _rateLimiterFactory.CreateRateLimiter("resource1", clientData);
        var isAllowedRequest = rateLimiterResource3.IsRequestAllowed();

        Assert.AreEqual(isAllowedRequest, result);
    }

    [Test]
    public void RateLimitResource3_USBased_Allowed()
    {
        var clientData = new ClientModel()
        {
            ClientId = "123",
            Region = "US"
        };

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>);

        var rateLimiterResource3 = _rateLimiterFactory.CreateRateLimiter("resource3", clientData);
        var isAllowedRequest = rateLimiterResource3.IsRequestAllowed();

        Assert.That(isAllowedRequest, Is.True);
    }

    [Test]
    public void RateLimitResource_Exception()
    {
        var clientData = new ClientModel();

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            _rateLimiterFactory.CreateRateLimiter("unknownResource", clientData);
        });

        Assert.That(ex.Message, Is.EqualTo("Cannot resolve resource mapping with url: unknownResource"));
    }
}