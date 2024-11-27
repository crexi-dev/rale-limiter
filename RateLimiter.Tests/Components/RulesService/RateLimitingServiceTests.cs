using FakeItEasy;
using FluentAssertions;
using RateLimiter.Components.CountryDataProvider;
using RateLimiter.Components.Repository;
using RateLimiter.Components.RuleService.Rules.RuleAllow1RequestForMatchingConfiguration;
using RateLimiter.Components.RuleService.Rules.RuleNRequestPerTimerange;
using RateLimiter.Components.RulesService.Rules.DummyRule;
using RateLimiter.Models;
using RateLimiter.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Components.RuleService.Tests
{
    public class RateLimitingServiceTests
    {
        [Fact]
        public async Task CanProcessRequestAsync_Should_Be_True_With_No_Rules()
        {
            var rules = new List<IRateLimitingRule>();
            var groups = new List<RateLimitingRuleGroup>();
            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groups }));

            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).Returns(new List<RulesService.Models.SelectRulesResult>()
            {

            });
            A.CallTo(() => engine.CanProcessRequestAsync(A<RateLimitingRequestData>._, A<List<string>>._)).CallsBaseMethod();

            var requestData = new RateLimitingRequestData()
            {

            };
            var groupList = new List<string>()
            {

            };

            var result = await engine.CanProcessRequestAsync(requestData, groupList);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanProcessRequestAsync_Should_Call_Each_Selected_Rule()
        {
            var rule1 = A.Fake<IRateLimitingRule>();
            var rule2 = A.Fake<IRateLimitingRule>();
            var rule3 = A.Fake<IRateLimitingRule>();
            var rules = new List<IRateLimitingRule>() { rule1, rule2, rule3 };

            var groups = new List<RateLimitingRuleGroup>();

            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groups }));


            A.CallTo(() => rule1.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).Returns(true);
            A.CallTo(() => rule2.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).Returns(true);
            A.CallTo(() => rule3.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).Returns(true);

            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).Returns(new List<RulesService.Models.SelectRulesResult>()
            {
                new RulesService.Models.SelectRulesResult() { Rule = rule1 },
                new RulesService.Models.SelectRulesResult() { Rule = rule2 },
                new RulesService.Models.SelectRulesResult() { Rule = rule3 },
                new RulesService.Models.SelectRulesResult() { Rule = rule3 },
            });

            A.CallTo(() => engine.CanProcessRequestAsync(A<RateLimitingRequestData>._, A<List<string>>._)).CallsBaseMethod();

            var requestData = new RateLimitingRequestData();
            var groupList = new List<string>();

            var result = await engine.CanProcessRequestAsync(requestData, groupList);


            A.CallTo(() => rule1.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => rule2.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => rule3.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).MustHaveHappenedTwiceExactly();

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public async Task CanProcessRequestAsync_Should_Return_Right_Result(bool rule1Result, bool rule2Result, bool expectedFinalResult)
        {
            var rule1 = A.Fake<IRateLimitingRule>();
            var rule2 = A.Fake<IRateLimitingRule>();
            var rules = new List<IRateLimitingRule>() { rule1, rule2 };

            var groups = new List<RateLimitingRuleGroup>();

            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groups }));


            A.CallTo(() => rule1.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).Returns(rule1Result);
            A.CallTo(() => rule2.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).Returns(rule2Result);

            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).Returns(new List<RulesService.Models.SelectRulesResult>()
            {
                new RulesService.Models.SelectRulesResult() { Rule = rule1 },
                new RulesService.Models.SelectRulesResult() { Rule = rule2 },
            });

            A.CallTo(() => engine.CanProcessRequestAsync(A<RateLimitingRequestData>._, A<List<string>>._)).CallsBaseMethod();

            var requestData = new RateLimitingRequestData();
            var groupList = new List<string>();

            var result = await engine.CanProcessRequestAsync(requestData, groupList);

            result.Should().Be(expectedFinalResult);
        }

        [Fact]
        public async Task CanProcessRequestAsyncTest_Rule_Throwing_Should_Return_true()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var countryProvider = A.Fake<ICountryDataProvider>();
            var rule1 = A.Fake<DummyRule>();
            var rules = new List<IRateLimitingRule>() { rule1 };


            var groupSets = new List<RateLimitingRuleGroup>()
            {
                new RateLimitingRuleGroup()
                {
                    Name = RateLimitingConstants.GlobalGroupName,
                    ConfigurationSets = new List<RateLimitingConfigurationSet>()
                    {
                        new RateLimitingConfigurationSet() { RuleName = nameof(DummyRule) },
                    }
                }
            };

            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groupSets }));

            A.CallTo(() => rule1.RunAsync(A<RateLimitingRequestData>._, A<RateLimitingRuleConfiguration>._)).Throws<Exception>();
            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).Returns(new List<RulesService.Models.SelectRulesResult>()
            {
                new RulesService.Models.SelectRulesResult()
                {
                    Rule = rule1
                }
            });
            A.CallTo(() => engine.CanProcessRequestAsync(A<RateLimitingRequestData>._, A<List<string>>._)).CallsBaseMethod();


            var requestData = new RateLimitingRequestData();
            var groupList = new List<string>();

            var result = await engine.CanProcessRequestAsync(requestData, groupList);

            result.Should().BeTrue();
        }




        [Fact]
        public void SelectRulesToProcessTest_Should_Return_Empty_With_Emty_Group_Set()
        {
            var rule1 = A.Fake<IRateLimitingRule>();
            var rule2 = A.Fake<IRateLimitingRule>();
            var rule3 = A.Fake<IRateLimitingRule>();
            var rules = new List<IRateLimitingRule>() { rule1, rule2, rule3 };

            var groupSets = new List<RateLimitingRuleGroup>();

            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groupSets }));
            
            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).CallsBaseMethod();


            var requestData = new RateLimitingRequestData();
            var groupList = new List<string>() { "group1", "group2" };

            var result = engine.SelectRulesToProcess(groupList);

            result.Count.Should().Be(0);
        }

        [Fact]
        public void SelectRulesToProcessTest_Should_Return_Empty_With_Emty_Group_List()
        {
            var rule1 = A.Fake<IRateLimitingRule>();
            var rule2 = A.Fake<IRateLimitingRule>();
            var rule3 = A.Fake<IRateLimitingRule>();
            var rules = new List<IRateLimitingRule>() { rule1, rule2, rule3 };

            var groupSets = new List<RateLimitingRuleGroup>()
            {
                new RateLimitingRuleGroup()
                {
                    Name = "group1",
                },
                new RateLimitingRuleGroup()
                {
                    Name = "group2",
                }
            };

            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groupSets }));

            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).CallsBaseMethod();


            var requestData = new RateLimitingRequestData();
            var groupList = new List<string>();

            var result = engine.SelectRulesToProcess(groupList);

            result.Count.Should().Be(0);
        }

        [Fact]
        public void SelectRulesToProcessTest_Should_Return_ExpectedRules()
        {
            var dataRepository = A.Fake<IDataRepository>();
            var countryProvider = A.Fake<ICountryDataProvider>();
            var rule1 = new DummyRule();
            var rule2 = new RuleNRequestPerTimerange(dataRepository);
            var rule3 = new RuleAllow1RequestForMatchingConfiguration(dataRepository, countryProvider);
            var rules = new List<IRateLimitingRule>() { rule1, rule2, rule3 };

            var groupSets = new List<RateLimitingRuleGroup>()
            {
                new RateLimitingRuleGroup()
                {
                    Name = RateLimitingConstants.GlobalGroupName,
                    ConfigurationSets = new List<RateLimitingConfigurationSet>()
                    {
                        new RateLimitingConfigurationSet() { RuleName = nameof(DummyRule) },
                    }
                },
                new RateLimitingRuleGroup()
                {
                    Name = "group1",
                    ConfigurationSets = new List<RateLimitingConfigurationSet>()
                    {
                        new RateLimitingConfigurationSet() { RuleName = nameof(RuleNRequestPerTimerange) }
                    }
                },
                new RateLimitingRuleGroup()
                {
                    Name = "group2",
                    ConfigurationSets = new List<RateLimitingConfigurationSet>()
                    {
                        new RateLimitingConfigurationSet() { RuleName = nameof(RuleAllow1RequestForMatchingConfiguration) },
                        new RateLimitingConfigurationSet() { RuleName = nameof(RuleNRequestPerTimerange) }
                    }
                }
            };

            var engine = A.Fake<RateLimitingService>(x => x.WithArgumentsForConstructor(new object[] { rules, groupSets }));

            A.CallTo(() => engine.SelectRulesToProcess(A<List<string>>._)).CallsBaseMethod();


            var requestData = new RateLimitingRequestData();
            var groupList = new List<string>() { RateLimitingConstants.GlobalGroupName, "group1", "group2"};

            var result = engine.SelectRulesToProcess(groupList);

            result.Count.Should().Be(4);
            result.Where(item => item.Rule.GetType() == typeof(DummyRule)).Count().Should().Be(1);
            result.Where(item => item.Rule.GetType() == typeof(RuleNRequestPerTimerange)).Count().Should().Be(2);
            result.Where(item => item.Rule.GetType() == typeof(RuleAllow1RequestForMatchingConfiguration)).Count().Should().Be(1);
        }

       
    }
}