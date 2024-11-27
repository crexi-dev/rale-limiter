using FakeItEasy;
using FluentAssertions;
using RateLimiter.Components.Repository;
using RateLimiter.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Components.RuleService.Rules.RuleNRequestPerTimerange.Tests
{
    public class RuleNRequestPerTimerangeTests
    {
        [Fact]
        public async Task RunAsyncTest_First_Call_Returns_True()
        {
            var dataRepository = A.Fake<IDataRepository>();

            A.CallTo(() => dataRepository.GetStateAsync<RuleNRequestPerTimerangeState>(A<string>._)).Returns(null as RuleNRequestPerTimerangeState);

            var engine = new RuleNRequestPerTimerange(dataRepository);

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get"
            };
            var ruleConfig = new RateLimitingRuleConfiguration();

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().BeTrue();

            A.CallTo(() => dataRepository.SaveStateAsync(A<string>._, A<RuleNRequestPerTimerangeState>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RunAsyncTest_Inside_Range_And_Allowed_By_Counter_Should_Return_True()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var engine = A.Fake<RuleNRequestPerTimerange>(x => x.WithArgumentsForConstructor(new object[] { dataRepository }));

            A.CallTo(() => engine.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).CallsBaseMethod();
            A.CallTo(() => engine.IsInsideRange(A<DateTime>._, A<TimeSpan>._)).Returns(true);
            A.CallTo(() => dataRepository.GetStateAsync<RuleNRequestPerTimerangeState>(A<string>._)).Returns(new RuleNRequestPerTimerangeState()
            {
                Counter = 1
            });

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get"
            };

            var ruleConfig = new RateLimitingRuleConfiguration()
            {
                Timerange = TimeSpan.FromSeconds(1),
                NumberOfRequests = 2
            };

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsyncTest_Inside_Range_And_Not_Allowed_By_Counter_Should_Return_False()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var engine = A.Fake<RuleNRequestPerTimerange>(x => x.WithArgumentsForConstructor(new object[] { dataRepository }));

            A.CallTo(() => engine.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).CallsBaseMethod();
            A.CallTo(() => engine.IsInsideRange(A<DateTime>._, A<TimeSpan>._)).Returns(true);
            A.CallTo(() => dataRepository.GetStateAsync<RuleNRequestPerTimerangeState>(A<string>._)).Returns(new RuleNRequestPerTimerangeState()
            {
                Counter = 100
            });

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get"
            };

            var ruleConfig = new RateLimitingRuleConfiguration()
            {
                Timerange = TimeSpan.FromSeconds(1),
                NumberOfRequests = 2
            };

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsyncTest_Outside_Range_Should_Return_True()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var engine = A.Fake<RuleNRequestPerTimerange>(x => x.WithArgumentsForConstructor(new object[] { dataRepository }));

            A.CallTo(() => engine.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).CallsBaseMethod();
            A.CallTo(() => engine.IsInsideRange(A<DateTime>._, A<TimeSpan>._)).Returns(false);
            A.CallTo(() => dataRepository.GetStateAsync<RuleNRequestPerTimerangeState>(A<string>._)).Returns(new RuleNRequestPerTimerangeState()
            {
                Counter = 100
            });

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get"
            };

            var ruleConfig = new RateLimitingRuleConfiguration()
            {
                Timerange = TimeSpan.FromSeconds(1),
                NumberOfRequests = 2
            };

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().BeTrue();

            A.CallTo(() => dataRepository.SaveStateAsync(A<string>._, A<RuleNRequestPerTimerangeState>._)).MustHaveHappenedOnceExactly();
        }
    }
}