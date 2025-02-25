using System;
using NUnit.Framework;
using RateLimiter.Constants;
using RateLimiter.Exceptions;
using RateLimiter.Factories;

namespace RateLimiter.Tests.Factories
{
    public class RateLimitRuleFactoryTest
    {
        [Test]
        public void CreateRule_Returns_ExpectedRule()
        {
            // Arrange
            int numRequestsAllowed = 1;
            var interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 10);
            var dataStoreFactory = new RateLimitDataStoreFactory();
            var ruleFactory = new RateLimitRuleFactory(dataStoreFactory);

            // Act
            var rateLimitRule = ruleFactory.CreateRule(
                RateLimitRuleTypes.RequestsPerTimeSpan,
                RateLimitDataStoreTypes.ConcurrentInMemory,
                DataStoreKeyTypes.RequestsPerResource,
                numRequestsAllowed,
                interval);

            // Assert
            Assert.NotNull(rateLimitRule);
        }

        [Test]
        public void CreateRule_Throws_NotImplementedException()
        {
            // Arrange
            RateLimitRuleTypes unimplementedRuleType = (RateLimitRuleTypes)10000;
            int numRequestsAllowed = 1;
            var interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 10);
            var dataStoreFactory = new RateLimitDataStoreFactory();
            var ruleFactory = new RateLimitRuleFactory(dataStoreFactory);

            // Act
            var rateLimitRule = () => ruleFactory.CreateRule(
                unimplementedRuleType,
                RateLimitDataStoreTypes.ConcurrentInMemory,
                DataStoreKeyTypes.RequestsPerResource,
                numRequestsAllowed,
                interval);

            // Assert
            Assert.Throws<RuleTypeNotImplementedException>(() => rateLimitRule());
        }
    }
}
