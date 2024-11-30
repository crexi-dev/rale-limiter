using Moq;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Factory;
using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
using RateLimiter.Core.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RateLimiter.Tests.BusinessLogic.Services.RateLimiter.Factory
{
	public class RuleFactoryTests
	{
		[Theory]
		[InlineData(RegionType.EU, 2)]
		[InlineData(RegionType.US, 1)]
		public void GetRulesByRegion_ShouldReturnRulesForSpecifiedRegion(RegionType regionType, int expectedCount)
		{
			// Arrange
			var rules = new List<IRuleService>
			{
				CreateMockRuleService(RegionType.EU),
				CreateMockRuleService(RegionType.US),
				CreateMockRuleService(RegionType.EU)
			};

			var ruleFactory = new RuleFactory(rules);

			// Act
			var result = ruleFactory.GetRulesByRegion(regionType).ToList();

			// Assert
			Assert.Equal(expectedCount, result.Count);
			Assert.All(result, ruleService => Assert.Equal(regionType, ruleService.RegionType));
		}

		[Theory]
		[InlineData(RuleType.RequestPerTimeSpan, RegionType.US)]
		[InlineData(RuleType.TimeSpanPassedSinceLastCall, RegionType.EU)]
		public void GetRule_ShouldReturnCorrectRuleByRuleType(RuleType expectedRuleType, RegionType regionType)
		{
			// Arrange
			var requestPerTimeRuleMock = new Mock<IRuleService>();
			requestPerTimeRuleMock.Setup(r => r.RuleType).Returns(RuleType.RequestPerTimeSpan);
			requestPerTimeRuleMock.Setup(r => r.RegionType).Returns(regionType);

			var lastCallRuleMock = new Mock<IRuleService>();
			lastCallRuleMock.Setup(r => r.RuleType).Returns(RuleType.TimeSpanPassedSinceLastCall);
			lastCallRuleMock.Setup(r => r.RegionType).Returns(regionType);

			var rules = new List<IRuleService>
			{
				requestPerTimeRuleMock.Object,
				lastCallRuleMock.Object
			};

			var ruleFactory = new RuleFactory(rules);

			// Act
			var result = ruleFactory.GetRule(expectedRuleType, regionType);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(expectedRuleType, result.RuleType);
		}

		private IRuleService CreateMockRuleService(RegionType regionType)
		{
			var mock = new Mock<IRuleService>();
			mock.Setup(r => r.RegionType).Returns(regionType);
			return mock.Object;
		}
	}
}
