using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

public class RateLimiterTests
{
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IRateLimitRuleRepository> _mockRuleRepository;
    private readonly RateLimiter _rateLimiter;
    delegate void OutDelegate<TIn, TOut>(TIn input, out TOut output);

    public RateLimiterTests()
    {
        _mockCache = new Mock<IMemoryCache>();
        _mockRuleRepository = new Mock<IRateLimitRuleRepository>();
        Mock<ILogger<RateLimiter>> mockLogger = new();
        var mockRateLimiterConfig = new Mock<IOptions<RateLimiterConfig>>(MockBehavior.Loose);
        mockRateLimiterConfig.Setup(m => m.Value).Returns(new RateLimiterConfig()
        {
            CacheExpirationInMinutes = 5
        });
        _rateLimiter = new RateLimiter(_mockCache.Object, _mockRuleRepository.Object, mockRateLimiterConfig.Object, mockLogger.Object);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_CacheMiss_FetchesRulesFromDatabase()
    {
        // Arrange
        var clientId = "client1";
        var resource = "resource1";
        var rules = new List<RateLimitRuleEntity>
        {
            new()
            {
                ClientId = clientId,
                Resource = resource,
                RuleType = RuleType.RequestCount,
                MaxRequests = 10,
                TimeSpan = TimeSpan.FromMinutes(1)
            }
        };

        // Setup mocks
        object? whatever;
        _mockCache
            .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out whatever))
            .Callback(new OutDelegate<object, object>((object _, out object v) =>
                v = new object())) // mocked value here (and/or breakpoint)
            .Returns(false);

        var cacheEntry = Mock.Of<ICacheEntry>();
        _mockCache
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntry);

        _mockRuleRepository.Setup(repo => repo.GetRulesByClientId(clientId))
            .ReturnsAsync(rules);

        // Act
        var result = await _rateLimiter.IsRequestAllowed(clientId, resource);

        // Assert
        Assert.True(result);
        _mockRuleRepository.Verify(repo => repo.GetRulesByClientId(clientId), Times.Once);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_NoRulesInDatabase_DenyRequest()
    {
        // Arrange
        var clientId = "client1";
        var resource = "resource1";

        // Setup mocks
        object? whatever;
        _mockCache
            .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out whatever))
            .Callback(new OutDelegate<object, object>((object _, out object v) =>
                v = new object())) // mocked value here (and/or breakpoint)
            .Returns(false);

        var cacheEntry = Mock.Of<ICacheEntry>();
        _mockCache
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntry);

        _mockRuleRepository.Setup(repo => repo.GetRulesByClientId(clientId))
            .ReturnsAsync(new List<RateLimitRuleEntity>());

        // Act
        var result = await _rateLimiter.IsRequestAllowed(clientId, resource);

        // Assert
        Assert.False(result);
        _mockRuleRepository.Verify(repo => repo.GetRulesByClientId(clientId), Times.Once);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_RulesAvailableInCache_AllowRequest()
    {
        // Arrange
        var clientId = "client1";
        var resource = "resource1";
        var rules = new List<RateLimitRuleEntity>
        {
            new()
            {
                ClientId = clientId,
                Resource = resource,
                RuleType = RuleType.RequestCount,
                MaxRequests = 10,
                TimeSpan = TimeSpan.FromMinutes(1)
            }
        };

        var ruleMock = new Mock<IRateLimitRule>();
        ruleMock.Setup(rule => rule.IsRequestAllowed(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        object? whatever;
        var callCount = 0;
        _mockCache
            .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out whatever))
            .Callback(new OutDelegate<object, object>((object _, out object v) =>
            {
                // On the first call, return an empty list
                v = callCount == 0 ? new List<RateLimitRuleEntity>() : 
                    // On the second call, return the populated list
                    rules;

                callCount++;
            }))
            .Returns(true);
        var cacheEntry = Mock.Of<ICacheEntry>();
        _mockCache
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntry);

        _mockRuleRepository.Setup(repo => repo.GetRulesByClientId(clientId))
            .ReturnsAsync(rules);

        await _rateLimiter.IsRequestAllowed(clientId, resource);

        // Act
        var result = await _rateLimiter.IsRequestAllowed(clientId, resource);

        // Assert
        Assert.True(result);
        _mockRuleRepository.Verify(repo => repo.GetRulesByClientId(clientId), Times.Once);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_InvalidInputs_ThrowsException()
    {
        // Arrange
        var clientId = "";
        var resource = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _rateLimiter.IsRequestAllowed(clientId, resource));
    }
}