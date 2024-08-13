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
public class FixedWindowRuleTest
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
        var value = (1, DateTime.MinValue);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);


        var rule = new FixedWindowRule(fixture.Create<TimeSpan>(), 2, storageMock.Object, fixture.Create<string>());
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsTrue(isRequestAllowed);

    }

    [Test]
    public async Task IsRequestAllowedAsync_WhenMaxRequestSetTo0_SholdReturnFalse()
    {
        var value = (1, DateTime.MinValue);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);


        var rule = new FixedWindowRule(fixture.Create<TimeSpan>(), 0, storageMock.Object, fixture.Create<string>());
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsFalse(isRequestAllowed);

    }

    [Test]
    public async Task IsRequestAllowedAsync_WhenRequestComes_SholdSetStorage()
    {
        var value = (1, DateTime.MinValue);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var groupId = fixture.Create<string>();
        var window = fixture.Create<TimeSpan>();
        var rule = new FixedWindowRule(window, 2, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        storageMock.Verify(x => x.SetAsync($"fw_{groupId}", It.Is<(int, DateTime)>(x => x.Item1 == 1), window, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsRequestAllowedAsync_OutOfLimit_SholdReturnFalse()
    {
        var value = (5, DateTime.UtcNow);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new FixedWindowRule(window, 5, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsFalse(isRequestAllowed);
    }

    [Test]
    public async Task IsRequestAllowedAsync_OutOfLimitOutOfWindow_SholdReturnTrue()
    {
        var value = (5, DateTime.MinValue);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new FixedWindowRule(window, 5, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.IsTrue(isRequestAllowed);
    }

    [Test]
    public async Task IsRequestAllowedAsync_OutOfWindow_ShouldResetRequestsInStorage()
    {
        var value = (5, DateTime.MinValue);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new FixedWindowRule(window, 5, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        storageMock.Verify(x => x.SetAsync($"fw_{groupId}", It.Is<(int, DateTime)>(x => x.Item1 == 1), window, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsRequestAllowedAsync_WithinLimits_ShouldReturnTrue()
    {
        var value = (5, DateTime.UtcNow);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new FixedWindowRule(window, 10, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        Assert.True(isRequestAllowed);
    }

    [Test]
    public async Task IsRequestAllowedAsync_WithinLimits_ShouldIncreaseValueInStorage()
    {
        var now = DateTime.UtcNow;
        var value = (5, now);
        storageMock.Setup(x => x.TryGetAsync(It.IsAny<string>(), out value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var groupId = fixture.Create<string>();
        var window = TimeSpan.FromMinutes(10);
        var rule = new FixedWindowRule(window, 10, storageMock.Object, groupId);
        var isRequestAllowed = await rule.IsRequestAllowedAsync(fixture.Create<HttpRequest>());

        storageMock.Verify(x => x.SetAsync($"fw_{groupId}", It.Is<(int, DateTime)>(x => x.Item1 == 6 && x.Item2 == now), window, It.IsAny<CancellationToken>()), Times.Once);
    }

}