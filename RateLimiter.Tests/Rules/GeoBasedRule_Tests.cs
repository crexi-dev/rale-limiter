using Moq;
using NUnit.Framework;
using RateLimiter.Geo;
using RateLimiter.Rules;
using RateLimiter.Storages;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Rules;

[TestFixture]
public class GeoBasedRule_Tests
{
    private GeoBasedRule _usGeoRule;
    private Mock<IRateLimitRule> _usRuleMock;
    private GeoBasedRule _euGeoRule;
    private Mock<IRateLimitRule> _euRuleMock;
    private Mock<IGeoService> _geoServiceMock;
    private readonly string _clientIdUS = "US123";
    private readonly string _clientIdEU = "EU456";
    private readonly string _actionKey = "action1";
    private Mock<IRateLimitStore> storeMock;

    [SetUp]
    public void Setup()
    {
        _usRuleMock = new Mock<IRateLimitRule>();
        _euRuleMock = new Mock<IRateLimitRule>();
        _geoServiceMock = new Mock<IGeoService>();
        storeMock = new Mock<IRateLimitStore>();

        _usGeoRule = new GeoBasedRule("US", _usRuleMock.Object, _geoServiceMock.Object);
        _euGeoRule = new GeoBasedRule("EU", _euRuleMock.Object, _geoServiceMock.Object);
    }

    [Test]
    public async Task AppliesUSRuleForUSClients()
    {
        // Arrange
        _=_geoServiceMock.Setup(s => s.GetLocationAsync(_clientIdUS)).ReturnsAsync("US");
        _=_usRuleMock.Setup(r => r.IsRequestAllowedAsync(_clientIdUS, _actionKey, storeMock.Object)).ReturnsAsync(true);

        // Act
        var result = await _usGeoRule.IsRequestAllowedAsync(_clientIdUS, _actionKey, storeMock.Object);

        // Assert
        Assert.IsTrue(result);
        _usRuleMock.Verify(r => r.IsRequestAllowedAsync(_clientIdUS, _actionKey, storeMock.Object), Times.Once);        
    }

    [Test]
    public async Task AppliesEURuleForEUClients()
    {
        // Arrange
        _=_geoServiceMock.Setup(s => s.GetLocationAsync(_clientIdEU)).ReturnsAsync("EU");
        _=_euRuleMock.Setup(r => r.IsRequestAllowedAsync(_clientIdEU, _actionKey, storeMock.Object)).ReturnsAsync(false);

        // Act
        var result = await _euGeoRule.IsRequestAllowedAsync(_clientIdEU, _actionKey, storeMock.Object);

        // Assert
        Assert.IsFalse(result);
        _euRuleMock.Verify(r => r.IsRequestAllowedAsync(_clientIdEU, _actionKey, storeMock.Object), Times.Once);
    }

    [Test]
    public async Task AllowsRequestsFromOtherLocations()
    {
        // Arrange
        var clientIdOther = "Other789";
        _=_geoServiceMock.Setup(s => s.GetLocationAsync(clientIdOther)).ReturnsAsync("Other");

        // Act
        var result = await _usGeoRule.IsRequestAllowedAsync(clientIdOther, _actionKey, storeMock.Object);

        // Assert
        Assert.IsTrue(result);
        _usRuleMock.Verify(r => r.IsRequestAllowedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IRateLimitStore>()), Times.Never);
        _euRuleMock.Verify(r => r.IsRequestAllowedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IRateLimitStore>()), Times.Never);
    }
}
