using Moq;
using NUnit.Framework;
using RateLimiter.Configuration;
using RateLimiter.Providers;
using RateLimiter.Rules;
using RateLimiter.Store;
using System.Linq;

namespace RateLimiter.Tests.Configuration
{
    public class RateLimiterConfigurationTests
    {
        private Mock<IDataStore> _mockDataStore;
        private Mock<IDateTimeProvider> _mockDataTimeProvider;
        private RateLimiterConfiguration _rateLimiterConfiguration;

        [SetUp]
        public void Setup()
        {
            _mockDataStore = new Mock<IDataStore>();
            _mockDataTimeProvider = new Mock<IDateTimeProvider>();
            _rateLimiterConfiguration = new RateLimiterConfiguration(_mockDataStore.Object, _mockDataTimeProvider.Object);
        }

        [Test]
        public void AddRule_ShouldAddRule_WhenUriIsNew()
        {
            var uri = "http://example.com";
            var rule = Mock.Of<IRateLimiterRule>();

            _rateLimiterConfiguration.AddRule(uri, rule);

            var rules = _rateLimiterConfiguration.GetRules(uri).ToList();
            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual(rule, rules[0]);
        }

        [Test]
        public void AddRule_ShouldAddRuleToExistingRules_WhenUriAlreadyHasRules()
        {
            var uri = "http://example.com";
            var rule1 = Mock.Of<IRateLimiterRule>();
            var rule2 = Mock.Of<IRateLimiterRule>();

            _rateLimiterConfiguration.AddRule(uri, rule1);

            _rateLimiterConfiguration.AddRule(uri, rule2);

            var rules = _rateLimiterConfiguration.GetRules(uri).ToList();
            Assert.AreEqual(2, rules.Count);
            Assert.AreEqual(rule1, rules[0]);
            Assert.AreEqual(rule2, rules[1]);
        }

        [Test]
        public void GetRules_ShouldReturnEmpty_WhenNoRulesExistForUri()
        {
            var uri = "http://example.com";

            var rules = _rateLimiterConfiguration.GetRules(uri);

            Assert.IsEmpty(rules);
        }

        [Test]
        public void GetRules_ShouldReturnRules_WhenRulesExistForUri()
        {
            var uri = "http://example.com";
            var rule = Mock.Of<IRateLimiterRule>();
            _rateLimiterConfiguration.AddRule(uri, rule);

            var rules = _rateLimiterConfiguration.GetRules(uri).ToList();

            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual(rule, rules[0]);
        }

        [Test]
        public void DataStore_ShouldReturnDataStoreInstance()
        {
            var dataStore = _rateLimiterConfiguration.DataStore;
            Assert.AreEqual(_mockDataStore.Object, dataStore);
        }
    }
}
