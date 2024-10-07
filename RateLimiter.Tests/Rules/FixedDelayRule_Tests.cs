using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Storages;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules;

[TestFixture]
public class FixedDelayRule_Tests
{
    private FixedDelayRule _rule;
    private Mock<IRateLimitStore> _storeMock;
    private readonly string _clientId = "client1";
    private readonly string _actionKey = "action1";

    [SetUp]
    public void Setup()
    {
        _rule = new FixedDelayRule(TimeSpan.FromSeconds(1)); // 1-second delay between requests
        _storeMock = new Mock<IRateLimitStore>();
    }

    [Test]
    public async Task EnforcesDelayBetweenRequests()
    {
        // Arrange
        var lastRequestTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(500); // Less than delay
        _=_storeMock.Setup(s => s.GetLastRequestTimeAsync(_clientId, _actionKey))
            .ReturnsAsync(lastRequestTime);

        // Act & Assert
        Assert.IsFalse(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));

        // Simulate last request time beyond the delay
        lastRequestTime = DateTime.UtcNow - TimeSpan.FromSeconds(2);
        _=_storeMock.Setup(s => s.GetLastRequestTimeAsync(_clientId, _actionKey))
            .ReturnsAsync(lastRequestTime);

        Assert.IsTrue(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));
    }

    [Test]
    public async Task AllowsFirstRequest()
    {
        // Arrange
        _=_storeMock.Setup(s => s.GetLastRequestTimeAsync(_clientId, _actionKey))
            .ReturnsAsync((DateTime?)null);

        // Act & Assert
        Assert.IsTrue(await _rule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object));
    }
}
