using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RateLimiter.Rules;
using NUnit.Framework;

namespace RateLimiter.Tests.Rules
{
    internal class RequestPerTimeSpanRuleTest
    {
        [Test]
        public void RequestPerTimeSpanRule_IsWithinLimit_ReturnsTrue_When_LimitIsNotExceeded()
        {
            // Arrange
            string userId = "user1";
            uint numRequestsAllowed = 1;
            var interval = new TimeSpan(0, 0, 5);
            var rateLimitRule = new RequestPerTimeSpanRule(numRequestsAllowed, interval);

            // Act
            var firstRequestAllowed = rateLimitRule.IsWithinLimit(userId);

            // Assert
            Assert.IsTrue(firstRequestAllowed);
        }

        [Test]
        public void RequestPerTimeSpanRule_IsWithinLimit_ReturnsFalse_When_LimitIsExceeded()
        {
            // Arrange
            string userId = "user1";
            uint numRequestsAllowed = 1;
            var interval = new TimeSpan(0, 0, 5);
            var rateLimitRule = new RequestPerTimeSpanRule(numRequestsAllowed, interval);

            // Act
            var firstRequestAllowed = rateLimitRule.IsWithinLimit(userId);
            var secondRequestAllowed = rateLimitRule.IsWithinLimit(userId);

            // Assert
            Assert.IsTrue(firstRequestAllowed);
            Assert.IsFalse(secondRequestAllowed);
        }


        [Test]
        public void RequestPerTimeSpanRule_IsWithinLimit_MultipleRequests()
        {
            // Arrange
            var delayInMs = 11;
            string userId = "user1";
            uint numRequestsAllowed = 1;
            var interval = new TimeSpan(0, 0, 0, 0, 10);
            var rateLimitRule = new RequestPerTimeSpanRule(numRequestsAllowed, interval);

            // Act
            var firstRequestAllowed = rateLimitRule.IsWithinLimit(userId);
            var secondRequestAllowed = rateLimitRule.IsWithinLimit(userId);
            var thirdRequestAllowed = () => rateLimitRule.IsWithinLimit(userId);
            var fourthRequestAllowed = () => rateLimitRule.IsWithinLimit(userId);

            // Assert
            Assert.IsTrue(firstRequestAllowed);
            Assert.IsFalse(secondRequestAllowed);
            Assert.That(() => thirdRequestAllowed(), Is.True.After(delayInMs));
            Assert.That(() => fourthRequestAllowed(), Is.False);
        }
    }
}
