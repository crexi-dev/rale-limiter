using Moq;
using RateLimitingLibrary.Rules;
using RateLimitingTests.Configurations;
using System;
using Xunit;

namespace RateLimitingTests.RuleTests
{
    public class SlidingWindowRuleTests
    {
        [Fact]
        public void SlidingWindowRule_WithinLimit_ReturnsAllowed()
        {
            var rule = new SlidingWindowRule(5, TimeSpan.FromMinutes(1));
            var request = MockConfiguration.CreateRequest("Client1", "ResourceA", DateTime.UtcNow);

            var result = rule.Evaluate(request);

            Assert.True(result.IsAllowed);
        }

        [Fact]
        public void SlidingWindowRule_ExceedsLimit_ReturnsDenied()
        {
            var rule = new SlidingWindowRule(1, TimeSpan.FromMinutes(1));
            var request = MockConfiguration.CreateRequest("Client1", "ResourceA", DateTime.UtcNow);

            rule.Evaluate(request);
            var result = rule.Evaluate(request);

            Assert.False(result.IsAllowed);
        }
    }
}