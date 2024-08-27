using NUnit.Framework;
using RateLimiter.Extensions;
using System;
using RateLimiter.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace RateLimiter.Tests
{
    [TestFixture, Description("FixedWindowRateLimitRule Unit Tests")]
    public class FixedWindowRateLimitRuleTest
	{
        private ILoggerFactory? _loggerFactory;
        private IMemoryCache? _memoryCache;
        private FixedWindowRateLimitRuleConfiguration? _fixedWindowRateLimitRuleConfiguration;

        [OneTimeSetUp]
        public void Init()
        {
            var serviceProvider = new ServiceCollection().AddLogging().AddMemoryCache().BuildServiceProvider();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();
            _fixedWindowRateLimitRuleConfiguration = new FixedWindowRateLimitRuleConfiguration();
        }

        [Test, Order(1)]
        public void TestFixedWindowRateLimitRuleConstructorWithAllNullArgumentsShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitRuleHelper.CreateFixedWindowRateLimitRule(null, null, null));
        }

        [Test, Order(2)]
        public void TestFixedWindowRateLimitRuleConstructorWithTwoNullArgumentsShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, null, null));
        }

        [Test, Order(3)]
        public void TestFixedWindowRateLimitRuleConstructorWithOneNullArgumentShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, _memoryCache, null));
        }

        [Test, Order(4)]
        public void TestFixedWindowRateLimitRuleConstructorWithNoNullArgumentsShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, _memoryCache, _fixedWindowRateLimitRuleConfiguration));
        }

        [Test, Order(5)]
        public void TestFixedWindowRateLimitRuleConstructorWithNoWindowConfigurationShouldThrowArgumentException()
        {
            if(_fixedWindowRateLimitRuleConfiguration != null)
            {
                _fixedWindowRateLimitRuleConfiguration.Limit = 10;
            }            
            Assert.Throws<ArgumentException>(() => RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, _memoryCache, _fixedWindowRateLimitRuleConfiguration));
        }

        [Test, Order(6)]
        public void TestFixedWindowRateLimitRuleSingleEvaluationShouldSucceed()
        {
            if (_fixedWindowRateLimitRuleConfiguration != null)
            {
                _fixedWindowRateLimitRuleConfiguration.Limit = 10;
                _fixedWindowRateLimitRuleConfiguration.Window = TimeSpan.FromSeconds(30);
            }
            IRateLimitRule rateLimitRule = RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, _memoryCache,
                _fixedWindowRateLimitRuleConfiguration);
            Assert.That(rateLimitRule, Is.Not.Null);
            string clientKey = "TestClientId1";
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.True);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 1, 0);
        }

        [Test, Order(7)]
        public void TestFixedWindowRateLimitRuleMultipleEvaluationShouldSucceed()
        {
            if (_fixedWindowRateLimitRuleConfiguration != null)
            {
                _fixedWindowRateLimitRuleConfiguration.Limit = 2;
                _fixedWindowRateLimitRuleConfiguration.Window = TimeSpan.FromSeconds(4);
            }
            IRateLimitRule rateLimitRule = RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, _memoryCache,
                _fixedWindowRateLimitRuleConfiguration);
            Assert.That(rateLimitRule, Is.Not.Null);
            string clientKey = "TestClientId2";
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.True);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 1, 0);
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.True);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 2, 0);
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.False);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 2, 1);
            _ = Is.True.After(5).Seconds;
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.False);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 2, 2);
        }
    }
}

