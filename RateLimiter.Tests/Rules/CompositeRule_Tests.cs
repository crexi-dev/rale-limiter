using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Storages;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules;

[TestFixture]
public class CompositeRule_Tests
{
    private CompositeRule _compositeRule;
    private Mock<IRateLimitRule> _rule1Mock;
    private Mock<IRateLimitRule> _rule2Mock;
    private readonly string _clientId = "client1";
    private readonly string _actionKey = "action1";
    private Mock<IRateLimitStore> _storeMock;

    [SetUp]
    public void Setup()
    {
        _rule1Mock = new Mock<IRateLimitRule>();
        _rule2Mock = new Mock<IRateLimitRule>();
        _storeMock = new Mock<IRateLimitStore>();

        _compositeRule = new CompositeRule([_rule1Mock.Object, _rule2Mock.Object]);
    }

    [Test]
    public async Task AllowsRequestWhenAllRulesAllow()
    {
        // Arrange
        _=_rule1Mock.Setup(r => r.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object)).ReturnsAsync(true);
        _=_rule2Mock.Setup(r => r.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object)).ReturnsAsync(true);

        // Act
        var result = await _compositeRule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeniesRequestWhenAnyRuleDenies()
    {
        // Arrange
        _=_rule1Mock.Setup(r => r.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object)).ReturnsAsync(true);
        _=_rule2Mock.Setup(r => r.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object)).ReturnsAsync(false);

        // Act
        var result = await _compositeRule.IsRequestAllowedAsync(_clientId, _actionKey, _storeMock.Object);

        // Assert
        Assert.IsFalse(result);
    }
}