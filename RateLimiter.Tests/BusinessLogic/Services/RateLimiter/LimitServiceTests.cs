using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter;
using RateLimiter.BusinessLogic.Services.RateLimiter.Factory;
using RateLimiter.BusinessLogic.Services.RateLimiter.Rules;
using RateLimiter.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Tests.BusinessLogic.Services.RateLimiter
{
	public class LimitServiceTests
	{
		private readonly IFixture _fixture;
		private readonly Mock<IRuleFactory> _ruleFactoryMock;
		private readonly LimitService _limitService;

		public LimitServiceTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
			_ruleFactoryMock = _fixture.Freeze<Mock<IRuleFactory>>();
			_limitService = new LimitService(_ruleFactoryMock.Object);
		}

		[Theory]
		[MemberData(nameof(DataForNoMatchedRules))]
		public async Task IsRequestReachedLimit_WhenNoMatchedRules_ShouldReturnFalseForRequestReachedLimit(
			List<RuleType> appliedRulesTypes,
			RegionType regionType,
			List<RuleType> ruleTypesByRegion)
		{
			// Arrange
			var appliedRuleTypes = appliedRulesTypes;
			var requestModel = _fixture
				.Build<RequestDto>()
				.With(x => x.RegionType, regionType)
				.Create();

			_ruleFactoryMock.Setup(x => x.GetRulesByRegion(requestModel.RegionType))
				.Returns(GetMockRuleServiceBehaviour(ruleTypesByRegion, regionType));

			// Act
			var result = await _limitService.IsRequestReachedLimit(appliedRuleTypes, requestModel);

			// Assert
			Assert.False(result);
		}

		[Theory]
		[MemberData(nameof(DataForAtLeastOneRuleFailed))]
		public async Task IsRequestReachedLimit_WhenAtLeastOneRullFails_ShouldReturnTrueForRequestReachedLimit(
			List<RuleType> appliedRuleTypes,
			RegionType regionType,
			List<AppliedToRequestResultModel> appliedToRequestResultModels)
		{
			// Arrange
			var requestModel = _fixture
				.Build<RequestDto>()
				.With(x => x.RegionType, regionType)
				.Create();

			var mockServices = GetMockRuleServiceBehaviourExtended(appliedToRequestResultModels, regionType, requestModel);

			_ruleFactoryMock.Setup(x => x.GetRulesByRegion(requestModel.RegionType)).Returns(mockServices);
			_ruleFactoryMock.Setup(x => x.GetRule(It.IsAny<RuleType>(), It.IsAny<RegionType>()))
				.Returns((RuleType ruleType, RegionType regionType) =>
				{
					if (Enum.GetValues(typeof(RuleType)).Cast<RuleType>().ToList().Contains(ruleType))
					{
						return mockServices.Single(x => x.RuleType == ruleType && x.RegionType == regionType);
					}

					throw new InvalidOperationException($"No matching RuleService found for {ruleType}");
				});

			// Act
			var result = await _limitService.IsRequestReachedLimit(appliedRuleTypes, requestModel);

			// Assert
			Assert.True(result);
		}

		[Theory]
		[MemberData(nameof(DataForAllRulesPassed))]
		public async Task IsRequestReachedLimit_WhenAllRulesPass_ShouldReturnFalseForRequestReachedLimit(
			List<RuleType> appliedRuleTypes,
			RegionType regionType,
			List<AppliedToRequestResultModel> appliedToRequestResultModels)
		{
			// Arrange
			var requestModel = _fixture
				.Build<RequestDto>()
				.With(x => x.RegionType, regionType)
				.Create();

			var mockServices = GetMockRuleServiceBehaviourExtended(appliedToRequestResultModels, regionType, requestModel);

			_ruleFactoryMock.Setup(x => x.GetRulesByRegion(requestModel.RegionType)).Returns(mockServices);
			_ruleFactoryMock.Setup(x => x.GetRule(It.IsAny<RuleType>(), It.IsAny<RegionType>()))
				.Returns((RuleType ruleType, RegionType regionType) =>
				{
					if (Enum.GetValues(typeof(RuleType)).Cast<RuleType>().ToList().Contains(ruleType))
					{
						return mockServices.Single(x => x.RuleType == ruleType && x.RegionType == regionType);
					}

					throw new InvalidOperationException($"No matching RuleService found for {ruleType}");
				});

			// Act
			var result = await _limitService.IsRequestReachedLimit(appliedRuleTypes, requestModel);

			// Assert
			Assert.False(result);
		}

		public static IEnumerable<object[]> DataForNoMatchedRules()
			=> new List<object[]>
			{
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall },
						RegionType.US,
						new List<RuleType>(),
					},
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall },
						RegionType.EU,
						new List<RuleType>(),
					},
					new object[]
					{
						new List<RuleType>{ RuleType.TimeSpanPassedSinceLastCall },
						RegionType.US,
						new List<RuleType>{ RuleType.RequestPerTimeSpan },
					},
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan },
						RegionType.EU,
						new List<RuleType>{ RuleType.TimeSpanPassedSinceLastCall },
					},
			};

		public static IEnumerable<object[]> DataForAtLeastOneRuleFailed()
			=> new List<object[]>
			{
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall },
						RegionType.US,
						new List<AppliedToRequestResultModel>
						{
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.RequestPerTimeSpan,
								AppliedToRequestResult = false,
							},
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.TimeSpanPassedSinceLastCall,
								AppliedToRequestResult = true,
							},
						}
					},
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall },
						RegionType.EU,
						new List<AppliedToRequestResultModel>
						{
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.RequestPerTimeSpan,
								AppliedToRequestResult = true,
							},
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.TimeSpanPassedSinceLastCall,
								AppliedToRequestResult = false,
							},
						}
					},
			};

		public static IEnumerable<object[]> DataForAllRulesPassed()
			=> new List<object[]>
			{
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall },
						RegionType.US,
						new List<AppliedToRequestResultModel>
						{
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.RequestPerTimeSpan,
								AppliedToRequestResult = true,
							},
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.TimeSpanPassedSinceLastCall,
								AppliedToRequestResult = true,
							},
						}
					},
					new object[]
					{
						new List<RuleType>{ RuleType.RequestPerTimeSpan, RuleType.TimeSpanPassedSinceLastCall },
						RegionType.EU,
						new List<AppliedToRequestResultModel>
						{
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.RequestPerTimeSpan,
								AppliedToRequestResult = true,
							},
							new AppliedToRequestResultModel
							{
								RuleType = RuleType.TimeSpanPassedSinceLastCall,
								AppliedToRequestResult = true,
							},
						}
					},
			};

		private List<IRuleService> GetMockRuleServiceBehaviour(
			List<RuleType> ruleTypesByRegion,
			RegionType regionType)
		{
			var services = new List<IRuleService>();
			if (ruleTypesByRegion.Any())
			{
				foreach (var ruleType in ruleTypesByRegion)
				{
					switch (ruleType)
					{
						case RuleType.RequestPerTimeSpan:
							var requestPerTimeRuleMock = _fixture.Create<Mock<IRuleService>>();
							requestPerTimeRuleMock.Setup(r => r.RuleType).Returns(ruleType);
							requestPerTimeRuleMock.Setup(r => r.RegionType).Returns(regionType);

							services.Add(requestPerTimeRuleMock.Object);
							break;
						case RuleType.TimeSpanPassedSinceLastCall:
							var lastCallRuleMock = _fixture.Create<Mock<IRuleService>>();
							lastCallRuleMock.Setup(r => r.RuleType).Returns(ruleType);
							lastCallRuleMock.Setup(r => r.RegionType).Returns(regionType);

							services.Add(lastCallRuleMock.Object);
							break;
						default:
							throw new Exception($"Invalid RuleType: {nameof(ruleType)}");
					}
				}
			}

			return services;
		}

		private List<IRuleService> GetMockRuleServiceBehaviourExtended(
			List<AppliedToRequestResultModel> appliedToRequestResultModels,
			RegionType regionType,
			RequestDto requestModel)
		{
			var services = new List<IRuleService>();
			foreach (var model in appliedToRequestResultModels)
			{
				switch (model.RuleType)
				{
					case RuleType.RequestPerTimeSpan:
						var requestPerTimeRuleMock = _fixture.Create<Mock<IRuleService>>();

						requestPerTimeRuleMock.Setup(r => r.ApplyToRequest(requestModel))
							.ReturnsAsync(model.AppliedToRequestResult);
						requestPerTimeRuleMock.Setup(r => r.RuleType).Returns(model.RuleType);
						requestPerTimeRuleMock.Setup(r => r.RegionType).Returns(regionType);

						services.Add(requestPerTimeRuleMock.Object);
						break;
					case RuleType.TimeSpanPassedSinceLastCall:
						var lastCallRuleMock = _fixture.Create<Mock<IRuleService>>();

						lastCallRuleMock.Setup(r => r.ApplyToRequest(requestModel)).ReturnsAsync(model.AppliedToRequestResult);
						lastCallRuleMock.Setup(r => r.RuleType).Returns(model.RuleType);
						lastCallRuleMock.Setup(r => r.RegionType).Returns(regionType);

						services.Add(lastCallRuleMock.Object);
						break;
					default:
						throw new Exception($"Invalid RuleType: {nameof(model.RuleType)}");
				}
			}

			return services;
		}
	}

	public class AppliedToRequestResultModel
	{
		public RuleType RuleType { get; set; }
		public bool AppliedToRequestResult { get; set; }
	}
}
