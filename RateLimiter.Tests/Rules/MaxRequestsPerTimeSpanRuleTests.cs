using System;
using System.Threading;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;

namespace RateLimiter.Tests.Rules
{
    [TestFixture]
    public class MaxRequestsPerTimeSpanRuleTests
    {
        [Test]
        public void Evaluate_LessThanMaxRequests_ReturnsAllowed()
        {
            // Arrange
            var rule = new MaxRequestsPerTimeSpanRule(5, TimeSpan.FromSeconds(10));
            var context = new RateLimitContext("client-123", "api/resource", Region.US);

            // Act and Assert
            for (int i = 0; i < 5; i++)
            {
                var response = rule.Evaluate(context);
                Assert.IsTrue(response.Allowed);
            }
        }

        [Test]
        public void MaxRequestsPerTimeSpanRule_MaxRequestsExceeded_ReturnsNotAllowed()
        {
            // Arrange
            var rule = new MaxRequestsPerTimeSpanRule(5, TimeSpan.FromSeconds(10));
            var context = new RateLimitContext("client-123", "api/resource", Region.US);

            // Act
            for (int i = 0; i < 5; i++)
            {
                rule.Evaluate(context);
            }
            var response = rule.Evaluate(context);

            // Assert
            Assert.IsFalse(response.Allowed);
            Assert.That(response.RejectedReasons, Has.Exactly(1).EqualTo(nameof(MaxRequestsPerTimeSpanRule)));
        }

        [Test]
        public void MaxRequestsPerTimeSpan_TimeSpanExceeded_ReturnsAllowed()
        {
            // Arrange
            var rule = new MaxRequestsPerTimeSpanRule(5, TimeSpan.FromSeconds(2));
            var context = new RateLimitContext("client-123", "api/resource", Region.US);

            // Act
            for (int i = 0; i < 5; i++)
            {
                rule.Evaluate(context);
            }
            Thread.Sleep(2100);
            var response = rule.Evaluate(context);

            // Assert
            Assert.IsTrue(response.Allowed);
        }

        [Test]
        public void MinimumTimeBetweenRequestsRule_MinimumTimeNotPassed_ReturnsNotAllowed()
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
        public void MinimumTimeBetweenRequestsRule_MinimumTimePassed_ReturnsAllowed()
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
