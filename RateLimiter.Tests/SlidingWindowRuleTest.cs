using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class SlidingWindowRuleTest
{
    private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<IRateLimiterStorage> storageMock;

    [SetUp]
    public void Setup()
    {
        storageMock = new Mock<IRateLimiterStorage>();
    }

    [Test]
    public async Task IsRequestAllowedAsync_WhenNoDataInStorage_SholdReturnTrue()
    {
        var value = DateTime.MinValue;
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);


        var rule = new SlidingWindowRule(fixture.Create<TimeSpan>(), storageMock.Object, fixture.Create<string>());
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsTrue(isRequestAllowed);

    }

    [Test]
    public async Task IsRequestAllowedAsync_WhenRequestComes_SholdSetStorage()
    {
        var value = DateTime.MinValue;
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var groupId = fixture.Create<string>();
        var window = fixture.Create<TimeSpan>();
        var rule = new SlidingWindowRule(window, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        storageMock.Verify(x => x.SetAsync($"sw_{groupId}", It.IsAny<DateTime>(), window, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsRequestAllowedAsync_OutOfLimit_SholdReturnFalse()
    {
        var value = DateTime.UtcNow;
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new SlidingWindowRule(window, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsFalse(isRequestAllowed);
    }

    [Test]
    public async Task IsRequestAllowedAsync_OutOfLimitOutOfWindow_SholdReturnTrue()
    {
        var value = DateTime.MinValue;
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new SlidingWindowRule(window, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsTrue(isRequestAllowed);
    }

    [Test]
    public async Task IsRequestAllowedAsync_OutOfWindow_ShouldResetRequestsInStorage()
    {
        var value = DateTime.MinValue;
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new SlidingWindowRule(window, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        storageMock.Verify(x => x.SetAsync($"sw_{groupId}", It.Is<DateTime>(x => x > value), window, It.IsAny<CancellationToken>()), Times.Once);
    }
}