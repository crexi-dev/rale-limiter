using FakeItEasy;
using FluentAssertions;
using RateLimiter.Components.CountryDataProvider;
using RateLimiter.Components.Repository;
using RateLimiter.Components.RulesService.Rules.RuleAllow1000RequestPerMinForUSMatchingControllerAndAction;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Components.RuleService.Rules.RuleAllow1RequestForMatchingConfiguration.Tests
{
    public class RuleAllow1RequestForMatchingConfigurationTests
    {
        [Fact]
        public async Task RunAsyncTest_First_Call_Returns_True()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var countryProvider = A.Fake<ICountryDataProvider>();

            string defaultCountry = "US";
            A.CallTo(() => countryProvider.GetByIp(A<IPAddress>._)).Returns(defaultCountry);
            A.CallTo(() => dataRepository.GetStateAsync<RuleAllow1RequestForMatchingConfigurationState>(A<string>._))
                .Returns(null as RuleAllow1RequestForMatchingConfigurationState);

            var engine = new RuleAllow1RequestForMatchingConfiguration(dataRepository, countryProvider);

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get"
            };
            var ruleConfig = new RateLimitingRuleConfiguration()
            {
                Controller = requestData.Controller,
                Action = requestData.Action,
                Country = defaultCountry
            };

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().BeTrue();

            A.CallTo(() => dataRepository.SaveStateAsync(A<string>._, A<RuleAllow1RequestForMatchingConfigurationState>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RunAsyncTest_Second_Call_Returns_False()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var countryProvider = A.Fake<ICountryDataProvider>();
            var engine = A.Fake<RuleAllow1RequestForMatchingConfiguration>(x => x.WithArgumentsForConstructor(new object[] { dataRepository, countryProvider }));

            string defaultCountry = "US";
            A.CallTo(() => countryProvider.GetByIp(A<IPAddress>._)).Returns(defaultCountry);
            A.CallTo(() => engine.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).CallsBaseMethod();
            A.CallTo(() => dataRepository.GetStateAsync<RuleAllow1RequestForMatchingConfigurationState>(A<string>._)).Returns(new RuleAllow1RequestForMatchingConfigurationState());

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get"
            };

            var ruleConfig = new RateLimitingRuleConfiguration()
            {
                Timerange = TimeSpan.FromSeconds(1),
                NumberOfRequests = 2,
                Controller = requestData.Controller,
                Action = requestData.Action,
                Country = defaultCountry
            };

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, true, true, true, true, true)]
        [InlineData(false, true, true, true, true, false)]
        [InlineData(true, false, true, true, true, false)]
        [InlineData(true, true, false, true, true, false)]
        [InlineData(true, true, true, false, true, false)]
        public async Task RunAsyncTest_Not_Running_Because_Controller_Or_Action_Or_Parameters_Dont_Match(
            bool countryMatch,
            bool controllerMatch,
            bool actionMatch,
            bool parametersMatch,
            bool expectedResult,
            bool expectedToSaveState)
        {
            var dataRepository = A.Fake<IDataRepository>();
            var countryProvider = A.Fake<ICountryDataProvider>();

            string defaultCountry = "US";
            A.CallTo(() => countryProvider.GetByIp(A<IPAddress>._)).Returns(defaultCountry);
            A.CallTo(() => dataRepository.GetStateAsync<RuleAllow1RequestForMatchingConfigurationState>(A<string>._))
                .Returns(null as RuleAllow1RequestForMatchingConfigurationState);

            var engine = new RuleAllow1RequestForMatchingConfiguration(dataRepository, countryProvider);

            var requestData = new RateLimitingRequestData()
            {
                Controller = "controller 1",
                Action = "Get",
                Parameters = new List<string>() { "1", "2" }
            };
            var ruleConfig = new RateLimitingRuleConfiguration()
            {
                Controller = controllerMatch ? requestData.Controller : "",
                Action = actionMatch ? requestData.Action : "",
                Country = countryMatch ? defaultCountry : "",
                Parameters = parametersMatch ? requestData.Parameters : null
            };

            var result = await engine.RunAsync(requestData, ruleConfig);

            result.Should().Be(expectedResult);

            if (expectedToSaveState)
            {
                A.CallTo(() => dataRepository.SaveStateAsync(A<string>._, A<RuleAllow1RequestForMatchingConfigurationState>._))
                .MustHaveHappened();
            }
            else
            {
                A.CallTo(() => dataRepository.SaveStateAsync(A<string>._, A<RuleAllow1RequestForMatchingConfigurationState>._))
                .MustNotHaveHappened();
            }
            
        }
    }
}