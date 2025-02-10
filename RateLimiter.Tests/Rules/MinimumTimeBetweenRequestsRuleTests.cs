using System;
using System.Threading;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Tests.Rules
{
    [TestFixture]
    public class MinimumTimeBetweenRequestsRuleTests
    {
        [Test]
        public void Evaluate_MinimumTimeNotPassed_ReturnsNotAllowed()
        {
            // Arrange
            var rule = new MinimumTimeBetweenRequestsRule(TimeSpan.FromSeconds(2));
            var context = new RateLimitContext("client-123", "api/resource", Region.US);

            // Act
            var response1 = rule.Evaluate(context);
            var response2 = rule.Evaluate(context);

            // Assert
            Assert.IsTrue(response1.Allowed);
            Assert.IsFalse(response2.Allowed);
            Assert.That(response2.RejectedReasons, Has.Exactly(1).EqualTo(nameof(MinimumTimeBetweenRequestsRule)));
        }

        [Test]
        public void Evaluate_MinimumTimePassed_ReturnsAllowed()
        {
            // Arrange
            var rule = new MinimumTimeBetweenRequestsRule(TimeSpan.FromSeconds(2));
            var context = new RateLimitContext("client-123", "api/resource", Region.US);

            // Act
            rule.Evaluate(context);
            Thread.Sleep(2100); 
            var response = rule.Evaluate(context);

            // Assert
            Assert.IsTrue(response.Allowed);
        }
	}
}
