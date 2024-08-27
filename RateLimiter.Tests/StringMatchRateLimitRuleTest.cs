using NUnit.Framework;
using RateLimiter.Extensions;
using System;
using RateLimiter.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace RateLimiter.Tests
{
    [TestFixture, Description("StringMatchRateLimitRule Unit Tests")]
    public class StringMatchRateLimitRuleTest
	{
        private ILoggerFactory? _loggerFactory;
        private StringMatchRateLimitRuleConfiguration? _stringMatchRateLimitRuleConfiguration;

        [OneTimeSetUp]
        public void Init()
        {
            var serviceProvider = new ServiceCollection().AddLogging().AddMemoryCache().BuildServiceProvider();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _stringMatchRateLimitRuleConfiguration = new StringMatchRateLimitRuleConfiguration();
        }

        [Test, Order(1)]
        public void TestStringMatchRateLimitRuleConstructorWithAllNullArgumentsShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitRuleHelper.CreateStringMatchRateLimitRule(null, null));
        }

        [Test, Order(2)]
        public void TestStringMatchRateLimitRuleConstructorWithOneNullArgumentShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitRuleHelper.CreateStringMatchRateLimitRule(_loggerFactory, null));
        }

        [Test, Order(3)]
        public void TestStringMatchRateLimitRuleConstructorWithNoNullArgumentsShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RateLimitRuleHelper.CreateStringMatchRateLimitRule(_loggerFactory, _stringMatchRateLimitRuleConfiguration));
        }

        [Test, Order(4)]
        public void TestStringMatchRateLimitRuleSingleEvaluationShouldSucceed()
        {
            if (_stringMatchRateLimitRuleConfiguration != null)
            {
                _stringMatchRateLimitRuleConfiguration.Match = "EU";
            }
            IRateLimitRule rateLimitRule = RateLimitRuleHelper.CreateStringMatchRateLimitRule(_loggerFactory, _stringMatchRateLimitRuleConfiguration);
            Assert.That(rateLimitRule, Is.Not.Null);
            string clientKey = "EU";
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.True);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 1, 0);
        }

        [Test, Order(5)]
        public void TestFixedWindowRateLimitRuleMultipleEvaluationShouldSucceed()
        {
            if (_stringMatchRateLimitRuleConfiguration != null)
            {
                _stringMatchRateLimitRuleConfiguration.Match = "EU";
            }
            IRateLimitRule rateLimitRule = RateLimitRuleHelper.CreateStringMatchRateLimitRule(_loggerFactory, _stringMatchRateLimitRuleConfiguration);
            Assert.That(rateLimitRule, Is.Not.Null);
            string clientKey = "EU";
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.True);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 1, 0);            
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.True);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 2, 0);
            clientKey = "US";
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.False);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 2, 1);
            Assert.That(rateLimitRule.Evaluate(clientKey), Is.False);
            RateLimitRuleTestHelper.VerifyRateLimitRuleAllowedDenied(rateLimitRule, 2, 2);
        }
    }
}

