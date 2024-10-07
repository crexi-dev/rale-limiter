using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Storages;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules;

[TestFixture]
public class FixedWindowRule_Tests
{
    private FixedWindowRule _rule;
    private Mock<IRateLimitStore> _storeMock;
    private readonly string _clientId = "client1";
    private readonly string _actionKey = "action1";

    [SetUp]
    public void Setup()
    {
        _rule = new FixedWindowRule(3, TimeSpan.FromSeconds(5)); // Allow 3 requests per 5 seconds
        _storeMock = new Mock<IRateLimitStore>();
    }

    [Test]
    public async Task AllowsRequestsWithinLimit()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        _=_storeMock.Setup(s => s.GetRequestTimesAsync(_clientId, _actionKey))
            .ReturnsAsync([]);

        // Act & Assert
        Assert.IsTrue(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));
        Assert.IsTrue(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));
        Assert.IsTrue(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));

        // Simulate that 3 requests have been made
        _=_storeMock.Setup(s => s.GetRequestTimesAsync(_clientId, _actionKey))
            .ReturnsAsync([currentTime, currentTime, currentTime]);

        Assert.IsFalse(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));
    }

    [Test]
    public async Task ResetsAfterWindow()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var oldTime = currentTime - TimeSpan.FromSeconds(6); // Outside the window
        _=_storeMock.Setup(s => s.GetRequestTimesAsync(_clientId, _actionKey))
            .ReturnsAsync([oldTime]);

        // Act & Assert
        Assert.IsTrue(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));

        // Verify that old timestamps are removed
        _storeMock.Verify(s => s.RemoveOldRequestTimesAsync(_clientId, _actionKey, It.IsAny<DateTime>()), Times.Once);
    }
}
