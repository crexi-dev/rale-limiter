using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using NUnit.Framework;
using RateLimiter.Configuration;
using RateLimiter.Extensions;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Interface;

namespace RateLimiter.Tests
{
    [TestFixture, Description("CompositeRateLimitRule Unit Tests")]
    public class CompositeRateLimitRuleTest
	{
        private ILoggerFactory? _loggerFactory;
        private IMemoryCache? _memoryCache;
        private FixedWindowRateLimitRuleConfiguration? _fixedWindowRateLimitRuleConfiguration;
        private StringMatchRateLimitRuleConfiguration? _stringMatchRateLimitRuleConfiguration;

        [OneTimeSetUp]
        public void Init()
        {
            var serviceProvider = new ServiceCollection().AddLogging().AddMemoryCache().BuildServiceProvider();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();
            _fixedWindowRateLimitRuleConfiguration = new FixedWindowRateLimitRuleConfiguration();
            _stringMatchRateLimitRuleConfiguration = new StringMatchRateLimitRuleConfiguration();
        }

        [Test, Order(1)]
        public void TestCompositeRateLimitRuleConstructorWithAllNullArgumentsShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitRuleHelper.CreateCompositeRateLimitRule(null));
        }

        [Test, Order(2)]
        public void TestCompositeRateLimitRuleAddWithNullArgumentsShouldThrowArgumentNullException()
        {
            if (_loggerFactory != null)
            {
                ICompositeRateLimitRule compositeRateLimitRule = RateLimitRuleHelper.CreateCompositeRateLimitRule(_loggerFactory);
                Assert.That(compositeRateLimitRule, Is.Not.Null);
                Assert.Throws<ArgumentNullException>(() => compositeRateLimitRule.Add(null));
            }
        }

        [Test, Order(3)]
        public void TestCompositeRateLimitRuleConstructorWithNoNullArgumentsShouldSucceed()
        {
            if(_loggerFactory != null)
            {
                ICompositeRateLimitRule compositeRateLimitRule = RateLimitRuleHelper.CreateCompositeRateLimitRule(_loggerFactory);
                Assert.That(compositeRateLimitRule, Is.Not.Null);
            }
        }

        [Test, Order(4)]
        public void TestCompositeRateLimitRuleNoConstituentRulesSingleEvaluationShouldThrowArgumentException()
        {
            if (_loggerFactory != null)
            {
                ICompositeRateLimitRule compositeRateLimitRule = RateLimitRuleHelper.CreateCompositeRateLimitRule(_loggerFactory);
                Assert.That(compositeRateLimitRule, Is.Not.Null);
                Assert.Throws<ArgumentException>(() => compositeRateLimitRule.Evaluate("US"));
            }
        }

        [Test, Order(5)]
        public void TestCompositeRateLimitRuleWithTwoConstituentRulesMultipleEvaluationShouldSucceed()
        {
            if (_loggerFactory != null)
            {
                ICompositeRateLimitRule compositeRateLimitRule = RateLimitRuleHelper.CreateCompositeRateLimitRule(_loggerFactory);
                Assert.That(compositeRateLimitRule, Is.Not.Null);
                compositeRateLimitRule.Add(CreateStringMatchRule());
                compositeRateLimitRule.Add(CreateFixedWindowRateLimitRule());
                Assert.That(compositeRateLimitRule.Evaluate("US"), Is.True);
                RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(compositeRateLimitRule, 2, 0);
                Assert.That(compositeRateLimitRule.Evaluate("EU"), Is.False);
                RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(compositeRateLimitRule, 3, 1);
            }
        }

        private IRateLimitRule CreateStringMatchRule()
        {
            if (_stringMatchRateLimitRuleConfiguration != null)
            {
                _stringMatchRateLimitRuleConfiguration.Match = "US";
            }
            return RateLimitRuleHelper.CreateStringMatchRateLimitRule(_loggerFactory, _stringMatchRateLimitRuleConfiguration);
        }

        private IRateLimitRule CreateFixedWindowRateLimitRule()
        {
            if (_fixedWindowRateLimitRuleConfiguration != null)
            {
                _fixedWindowRateLimitRuleConfiguration.Limit = 4;
                _fixedWindowRateLimitRuleConfiguration.Window = TimeSpan.FromSeconds(4);
            }
            return RateLimitRuleHelper.CreateFixedWindowRateLimitRule(_loggerFactory, _memoryCache,
                _fixedWindowRateLimitRuleConfiguration);
        }
    }
}

