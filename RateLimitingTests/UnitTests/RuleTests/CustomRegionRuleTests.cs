using Moq;
using RateLimitingLibrary.Rules;
using RateLimitingTests.Configurations;
using System;
using Xunit;

namespace RateLimitingTests.RuleTests
{
    public class CustomRegionRuleTests
    {
        [Fact]
        public void CustomRegionRule_AllowsRequest_ForNonRestrictedTime()
        {
            var rule = new CustomRegionRule();
            var request = MockConfiguration.CreateRequest("US123", "ResourceA", DateTime.UtcNow);

            var result = rule.Evaluate(request);

            Assert.True(result.IsAllowed);
        }

        [Fact]
        public void CustomRegionRule_DeniesRequest_ForRestrictedTime()
        {
            var rule = new CustomRegionRule();
            var request = MockConfiguration.CreateRequest("US123", "ResourceA", DateTime.UtcNow.AddSeconds(-DateTime.UtcNow.Second % 2));

            var result = rule.Evaluate(request);

            Assert.False(result.IsAllowed);
        }
    }
}