using NUnit.Framework;
using RateLimiter.Storages;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Storage;

[TestFixture]
public class InMemoryStore_Tests
{
    private InMemoryStore _store;
    private readonly string _clientId = "client1";
    private readonly string _actionKey = "action1";

    [SetUp]
    public void Setup()
    {
        _store = new InMemoryStore();
    }

    [Test]
    public async Task AddAndGetRequestTimesAsync()
    {
        // Act
        await _store.AddRequestTimeAsync(_clientId, _actionKey, DateTime.UtcNow);
        var times = await _store.GetRequestTimesAsync(_clientId, _actionKey);

        // Assert
        Assert.AreEqual(1, times.Count);
    }

    [Test]
    public async Task RemoveOldRequestTimesAsync()
    {
        // Arrange
        var oldTime = DateTime.UtcNow - TimeSpan.FromSeconds(10);
        await _store.AddRequestTimeAsync(_clientId, _actionKey, oldTime);

        // Act
        await _store.RemoveOldRequestTimesAsync(_clientId, _actionKey, DateTime.UtcNow - TimeSpan.FromSeconds(5));
        var times = await _store.GetRequestTimesAsync(_clientId, _actionKey);

        // Assert
        Assert.AreEqual(0, times.Count);
    }

    [Test]
    public async Task SetAndGetLastRequestTimeAsync()
    {
        // Act
        var currentTime = DateTime.UtcNow;
        await _store.SetLastRequestTimeAsync(_clientId, _actionKey, currentTime);
        var lastTime = await _store.GetLastRequestTimeAsync(_clientId, _actionKey);

        // Assert
        Assert.AreEqual(currentTime, lastTime);
    }
}
