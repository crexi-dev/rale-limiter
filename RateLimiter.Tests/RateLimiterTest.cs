using NuGet.Frameworks;
using NUnit.Framework;
using RateLimiter.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [Test, Order(1)]
    public void TestConfig()
    {
        var config = Limiter.Instance.Config;
        int countOfDefaults = config.Rules.Where(r => r.IsDefault).Count();

        // assert that countOfDefaults is no more than 1
        Assert.That(countOfDefaults, Is.EqualTo(1));
    }

    [Test, Order(2)]
    public void TestUSAUserOnMatching()
    {
        string targetResource = "/api/listings";

        string userToken = "user_USA";

        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>> { 
            new KeyValuePair<string, string>("Accept-Language", "en-US") 
        };

        IDictionary<string, string> headersUSA = GetHeaders(list);

        LimiterConfig config = GetConfigForAcceptLanguages(targetResource);

        /// Simple test  US-based users
        LimitResult result = Run(userToken, targetResource, config, headersUSA);

        Assert.Multiple(() =>
        {
            Assert.IsNull(result.FailReason, $"Expected FailReason to be Null");

            Assert.AreEqual(result, LimitResult.Success, "Expected Success Results");

        });

    }

    [Test, Order(3)]
    public void TestUSAUserOnNonMatching()
    {

        string targetResource = "/api/listings";

        string userToken = "user_USA";

        IDictionary<string, string> headersUSA = GetHeaders(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Accept-Language", "en-US") });

        LimiterConfig config = GetConfigForAcceptLanguages(targetResource);

        LimitResult resultTwo = Run(userToken, "/api/users", config, headersUSA);

        Assert.Multiple(() =>
        {
            Assert.IsNull(resultTwo.FailReason, $"Expected FailReason to be Null");

            Assert.AreEqual(resultTwo, LimitResult.Unmatched, "Expected Result to be Unmatched");

        });
    }

    [Test, Order(4)]
    public void TestUSBasedMultiReqs()
    {
        int successfulCalls = 0;
        int numberOfCalls = 0;

        string targetResource = "/api/listings";

        //define user 
        string userToken = "user_USA";

        IDictionary<string, string> headersUSA = GetHeaders(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Accept-Language", "en-US") });

        LimiterConfig config = GetConfigForAcceptLanguages(targetResource);

        #region - Loop 1: Pass both requests -
        LimitResult? resultLoopOne = null;

        for (int i = 0; i < 2; i++)
        {
            resultLoopOne = Run(userToken + "LoopOne", targetResource, config, headersUSA);

            if (resultLoopOne.IsSuccessful)
            {
                successfulCalls++;
            }
            numberOfCalls++;
        }

        Assert.Multiple(() =>
        {
            Assert.AreEqual(numberOfCalls, successfulCalls, $"successfulCalls ({successfulCalls}) expected to equal numberOfCalls {numberOfCalls} ");
            Assert.AreEqual(resultLoopOne, LimitResult.Success, "Expected Result to be Unmatched");
        });

        numberOfCalls = 0;
        successfulCalls = 0;
        #endregion

    }


    [Test, Order(5)]
    public void TestUSBasedTripHighRate()
    {
        int successfulCalls = 0;
        int numberOfCalls = 0;

        string targetResource = "/api/listings";

        //define user 
        string userToken = "user_USA";

        IDictionary<string, string> headersUSA = GetHeaders(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Accept-Language", "en-US") });

        LimiterConfig config = GetConfigForAcceptLanguages(targetResource);

        #region - Loop Two - 3 loops to trigger last fail -
        LimitResult? resultLoopTwo = null;

        for (int i = 0; i < 3; i++)
        {
            resultLoopTwo = Run(userToken + "LoopTwo", targetResource, config, headersUSA);

            if (resultLoopTwo.IsSuccessful)
            {
                successfulCalls++;
            }
            numberOfCalls++;
        }

        Assert.Multiple(() =>
        {
            Assert.NotNull(resultLoopTwo.FailReason, "Expected FailReason to Not be Null");

            Assert.IsTrue(resultLoopTwo.FailReason.Contains("Rate limit exceeded"), "Expected FailReason to contain 'Rate limit exceeded'");
            Assert.IsTrue(resultLoopTwo.FailReason.Contains("Second"), "Expected FailReason to contain 'Second'");
            Assert.AreNotEqual(numberOfCalls, successfulCalls, $"successfulCalls ({successfulCalls}) expected to not equal numberOfCalls {numberOfCalls} ");
        });
        numberOfCalls = 0;
        successfulCalls = 0;

        #endregion     
    }

    [Test, Order(6)]
    public async Task TestUSBasedTripSecondaryHighRate()
    {
        int successfulCalls = 0;
        int numberOfCalls = 0;

        string targetResource = "/api/listings";

        //define user 
        string userToken = "user_USA_"+DateTime.Now.Ticks.ToString();

        IDictionary<string, string> headersUSA = GetHeaders(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Accept-Language", "en-US") });

        LimiterConfig config = GetConfigForAcceptLanguages(targetResource);

        #region - Loop Three: 4 loops to trigger fail on second limit by Mintue -
        LimitResult? resultLoopThree = null;

        for (int i = 0; i < 4; i++)
        {
            resultLoopThree = Run(userToken + "LoopThree", targetResource, config, headersUSA);

            if (resultLoopThree.IsSuccessful)
            {
                successfulCalls++;
            }
            numberOfCalls++;
            await Task.Delay(500);
        }

        Assert.Multiple(() =>
        {
            Assert.NotNull(resultLoopThree.FailReason, "Expected FailReason to Not be Null");

            Assert.IsTrue(resultLoopThree.FailReason.Contains("for Minute"), "Expected FailReason to contain Minute");

            Assert.AreNotEqual(numberOfCalls, successfulCalls, $"SuccessfulCalls in Loop Three ({successfulCalls}) expected to not equal numberOfCalls {numberOfCalls} ");

        });
        numberOfCalls = 0;
        successfulCalls = 0;

        #endregion

    }

    [Test, Order(7)]
    public async Task TestUSBasedTripSpacing()
    {
        int successfulCalls = 0;
        int numberOfCalls = 0;

        string targetResource = "/api/listings";

        //define user 
        string userToken = "user_USA";

        IDictionary<string, string> headersUSA = GetHeaders(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Accept-Language", "en-US") });

        LimiterConfig config = GetConfigForAcceptLanguages(targetResource);

        #region - Loop Four:  -
        LimitResult? resultLoopFour = null;

        config.Rules[0].Limits = new List<Limit>()
            {
                new Limit()
                {
                    Type = LimitType.TimeWindow,
                    WindowType = TimeWindowType.Second,
                    RequestLimit = 20
                },
                new Limit()
                {
                    Type = LimitType.TimeWindow,
                    WindowType = TimeWindowType.Minute,
                    RequestLimit = 30
                },
                new Limit()
                {
                    Type = LimitType.RequestSpacing,
                    Spacing = TimeSpan.FromMilliseconds(750),
                }
            };



        for (int i = 0; i < 4; i++)
        {
            resultLoopFour = Run(userToken + "LoopFour", targetResource, config, headersUSA);

            if (resultLoopFour.IsSuccessful)
            {
                successfulCalls++;
            }
            numberOfCalls++;
            if (i < 2)
            {
                await Task.Delay(500);
            }

        }

        Assert.Multiple(() =>
        {
            Assert.NotNull(resultLoopFour.FailReason, "Expected FailReason to Not be Null");

            Assert.IsTrue(resultLoopFour.FailReason.Contains("Time Spacing required"), "Expected FailReason to contain 'Time Spacing required'");

            Assert.AreNotEqual(numberOfCalls, successfulCalls, $"successfulCalls ({successfulCalls}) expected to not equal numberOfCalls {numberOfCalls} ");

        });
        numberOfCalls = 0;
        successfulCalls = 0;

        #endregion

    }

    #region - Tests for the Config Files -

    [Test, Order(8)]
    [Category("ConfigFileTest")]
    [TestCase("/api/v1/test", true, TestName = "TestForMinuteRate_DefaultRule")]
    [TestCase("/api/users", false, TestName = "TestForMinuteRate_MatchedRule")]
    public async Task TestForMinuteRate(string endPoint, bool shouldBeDefaultRule)
    {
        string userToken = "user1";
        string apiEndpoint = endPoint;
        List<LimitResult> failedResults = new List<LimitResult>();
        List<LimitResult> passedResults = new List<LimitResult>();
        
        int numberOfCalls = 60;
        int successfulCalls = 0;

        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("HTTP_USER_AGENT", "Something OS Mobile Windows")
        };

        IDictionary<string, string> headers = GetHeaders(list);


        for (int i = 0; i < numberOfCalls; i++)
        {
            LimitResult result = Limiter.Instance.ApplyRateLimit(userToken, apiEndpoint, headers);

            if (result.IsSuccessful)
            {
                successfulCalls++;
                passedResults.Add(result);
            }

            if (i < numberOfCalls - 1)
            {
                await Task.Delay(1000); // delay 1
            }
        }       

        Assert.Multiple(() =>
        {
            Assert.AreEqual(successfulCalls, numberOfCalls, $"Total calls should equal {numberOfCalls}");
            for (int i = 0; i < passedResults.Count; i++)
            {
                Assert.That(passedResults[i].IsSuccessful, Is.True, $"Result at index {i} is not successful");
                Assert.That(passedResults[i].FailReason, Is.Null, $"Result at index {i} FailReason is Not Null");
                Assert.AreEqual(passedResults[i].Rule.IsDefault, shouldBeDefaultRule, $"The Rule IsDefault should be {shouldBeDefaultRule}");
            }
        });

        passedResults.Clear();
        failedResults.Clear();

        successfulCalls = 0;

        for (int i = 0; i < numberOfCalls; i++)
        {
            LimitResult result = Limiter.Instance.ApplyRateLimit(userToken + "Two", apiEndpoint, headers);

            if (result.IsSuccessful)
            {
                successfulCalls++;
                passedResults.Add(result);
            }
            else
            {
                failedResults.Add(result);
                break;
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(successfulCalls, Is.Not.EqualTo(numberOfCalls), $"successfulCalls should not equal {numberOfCalls}");
            for (int i = 0; i < failedResults.Count; i++)
            {
                Assert.That(failedResults[i].IsSuccessful, Is.False, $"Result at index {i} is successful");
                Assert.That(failedResults[i].FailReason, Is.Not.Null, $"Result at index {i} has a non-empty FailReason");
                Assert.IsTrue(failedResults[i].FailReason.Contains("Time Spacing required"), $"Expecting 'Time Spacing required' to appear in FailReason");
            }
        });

    }

   
    #endregion

    [Test, Order(9)]
    public async Task TestDefaultRuleMultiSpacing()
    {
        string userToken = "user1_" + DateTime.Now.Ticks;
        string apiEndpoint = "/api/v1/test";
        List<LimitResult> failedResults = new List<LimitResult>();
        IDictionary<string, string> headers = new Dictionary<string, string>();
        int numberOfCalls = 10;
        int successfulCalls = 0;
        int waitCounts = 0;
        int shortWaits = 0;

        LimiterConfig config = new LimiterConfig() { Rules = new List<Rule>() };
        config.Rules.Add(new Rule()
        {
            Name = "Rule with Spacing",
            IsDefault = true,
            Limits = new List<Limit>()
            {
                new Limit()
                {
                    Type = LimitType.RequestSpacing,
                    Spacing = TimeSpan.FromMilliseconds(500),
                }
            }
        });

        for (int i = 0; i < numberOfCalls; i++)
        {
            var result = Limiter.Instance.ApplyRateLimit(userToken, apiEndpoint, headers, config);

            if (result.IsSuccessful)
            {
                successfulCalls++;
            }
            else
            {
                failedResults.Add(result);
            }

            if (i < (numberOfCalls / 2) - 1)
            {
                await Task.Delay(1000);
                waitCounts++;
            }
            else
            {
                await Task.Delay(400);
                shortWaits++;
            }
        }
        Console.WriteLine($"{waitCounts} - {shortWaits}");



        Assert.Multiple(() =>
        {
            Assert.That(successfulCalls, Is.EqualTo((numberOfCalls / 2)), $"Total good calls should equal {(numberOfCalls / 2)}");

            for (int i = 0; i < failedResults.Count; i++)
            {
                Assert.That(failedResults[i].IsSuccessful, Is.False, $"Result at index {i} is Successful");
                Assert.That(failedResults[i].FailReason, Is.Not.Null, $"Result at index {i} has No FailReason");
                Assert.IsTrue(failedResults[i].FailReason.Contains("Time Spacing required"), $"Expecting 'Time Spacing required' to appear in FailReason");
            }
        });

    }



    [Test, Order(10)]
    public async Task TestForSpacingAndRate()
    {
        string userToken = "user1_"+DateTime.Now.Ticks.ToString();
        string apiEndpoint = "/api/users";
        IDictionary<string, string> headers = new Dictionary<string, string>();
        List<LimitResult> failedResults = new List<LimitResult>();
        int numberOfCalls = 10;
        int expectedFails = 5;

        int successfulCallsExpected = numberOfCalls - expectedFails;
        int successfulCallsActual = 0;

        TestContext.WriteLine($"numberOfCalls: {numberOfCalls}, expectedFails: {expectedFails}, successfulCallsExpected: {successfulCallsExpected}");

        LimiterConfig config = new LimiterConfig() { Rules = new List<Rule>() };

        config.Rules.Add(new Rule()
        {
            Name = "Rules for Users",
            Match = new Match { ApiUrl = apiEndpoint },
            Limits = new List<Limit>()
            {
                new Limit()
                {
                    Type = LimitType.RequestSpacing,
                    Spacing = TimeSpan.FromMilliseconds(500),
                },
                new Limit()
                {
                    Type = LimitType.TimeWindow,
                    WindowType = TimeWindowType.Hour,
                    RequestLimit = successfulCallsExpected
                }
            }
        });

        for (int i = 0; i < numberOfCalls; i++)
        {
            var result = Limiter.Instance.ApplyRateLimit(userToken, apiEndpoint, headers, config);

            if (result.IsSuccessful)
            {
                successfulCallsActual++;
            }
            else
            {
                failedResults.Add(result);
            }

            await Task.Delay(600);
        }

        Assert.Multiple(() =>
        {
            Assert.AreEqual(successfulCallsExpected, successfulCallsActual, $"Expected successfulCalls to be {successfulCallsActual}");

            for (int i = 0; i < failedResults.Count; i++)
            {
                Assert.That(failedResults[i].IsSuccessful, Is.False, $"Result at index {i} is Successful");
                Assert.That(failedResults[i].FailReason, Is.Not.Null, $"Result at index {i} has No FailReason");
                Assert.IsTrue(failedResults[i].FailReason.Contains("Rate limit exceeded"), $"Expecting 'Rate limit exceeded' to appear in FailReason");
                Assert.IsTrue(failedResults[i].FailReason.Contains("Rate limit exceeded"), $"Expecting 'Hour' to appear in FailReason");
            }
        });

    }


    [Test, Order(11)]
    [TestCase("Something OS; Mobile Something Else", "Mobile", false, TestName = "TestForUserAgentMobile")]
    [TestCase("Something OS; Desktop Something Else", "Mobile",true, TestName = "TestForUserAgentMobileUnMatched")]
    public void TestForUserAgent(string userAgent, string conditionPattern, bool shouldBeUnmatched)
    {
        IDictionary<string, string> headersUSA = GetHeaders(new List<KeyValuePair<string, string>> 
        {   new KeyValuePair<string, string>("Accept-Language", "en-US"),
            new KeyValuePair<string, string>("HTTP_USER_AGENT", userAgent)
        });

        List<Condition> conditions = new List<Condition>
        {
            new Condition{ Input = "HTTP_USER_AGENT", Pattern=conditionPattern}
        };

        LimitResult result = RunWithConditions("userTokenAny", userHeaders: headersUSA, defaultConditions: conditions);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(result.IsSuccessful, true, "Expected successfulCalls to be true");
            if(shouldBeUnmatched)
            {
                Assert.AreEqual(result, LimitResult.Unmatched, "Expected Matched Results");
            }
            else
            {
                Assert.AreNotEqual(result, LimitResult.Unmatched, "Expected Matched Results");
            }
            
        });      
        
    }

   
    [Test, Order(20)]
    public void TestLanguagePatterns()
    {
        TestForUsersLanguages("user1_language", headerLanguage: "en-US", conditionLanguage: "en");
        TestForUsersLanguages("user1_language", headerLanguage: "en-US", conditionLanguage: "en-US", delegate (LimitResult result)
        {
            //this one fails b/c it was for same user
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result.FailReason, $"Expected FailReason to have value");
                Assert.AreEqual(result.IsSuccessful, false, $"Expected successful Call to be false");
                Assert.AreNotEqual(result, LimitResult.Success, "Not Expecting 'Success' Results");
            });

        });
        TestForUsersLanguages("user2", headerLanguage: "en-US", conditionLanguage: "en-US");
        TestForUsersLanguages("userEnGb1", headerLanguage: "en-GB", conditionLanguage: "en");

    }

    private void TestForUsersLanguages(string userToken, string headerLanguage = "en-US", string conditionLanguage = "en", Action<LimitResult>? asserts = null)
    {
        IDictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("HTTP_USER_AGENT", "Something Something; Mobile And Browser 585");
        headers.Add("Accept-Language", headerLanguage);

        List<Condition> conditions = new List<Condition>
            {
                new Condition{ Input = "Accept-Language", Pattern=conditionLanguage}
            };

        LimitResult result = RunWithConditions(userToken, userHeaders: headers, defaultConditions: conditions);

        if (asserts != null)
        {
            asserts(result);
        }
        else
        {
            Assert.Multiple(() =>
            {

                Assert.IsNull(result.FailReason, $"Expected FailReason to be empty");
                Assert.AreEqual(result.IsSuccessful, true, $"Expected successful Call to be true for {headerLanguage} and {conditionLanguage}");
                Assert.AreEqual(result, LimitResult.Success, "Expected Success Results");
                Assert.AreNotEqual(result, LimitResult.Unmatched, "Expected Matched Results");                

            });
        }


        TestContext.WriteLine($"Success Results returned: {result == LimitResult.Success}");

    }

    private LimitResult RunWithConditions(string userToken, string apiEndpoint = "/api/users", IDictionary<string, string>? userHeaders = null, List<Condition>? defaultConditions = null)
    {   
        IDictionary<string, string> headers = userHeaders ?? new Dictionary<string, string>();
        List<Condition> conditions = defaultConditions ?? new List<Condition>
            {
                new Condition{ Input = "HTTP_USER_AGENT", Pattern="Mobile"}
            };

        LimiterConfig config = new LimiterConfig() { Rules = new List<Rule>() };
        config.Rules.Add(new Rule()
        {
            Name = "Rules for Conditional Testing",
            Match = new Match { ApiUrl = apiEndpoint },

            Limits = new List<Limit>()
            {
                new Limit()
                {
                    Type = LimitType.RequestSpacing,
                    Spacing = TimeSpan.FromMilliseconds(500),
                },
                new Limit()
                {
                    Type = LimitType.TimeWindow,
                    WindowType = TimeWindowType.Hour,
                    RequestLimit = 50
                }
            },
            Conditions = conditions

        });

        LimitResult result = Limiter.Instance.ApplyRateLimit(userToken, apiEndpoint, headers, config);

        return result;
    }

    private static IDictionary<string, string> GetHeaders(List<KeyValuePair<string, string>> kvps)
    {
        Dictionary<string, string> headersUSA = new Dictionary<string, string>(kvps);

        return headersUSA;
    }

    private static LimiterConfig GetConfigForAcceptLanguages(string targetResource)
    {
        //define the conditions and api to match
        List<Condition> usersUSA = new List<Condition>
        {
            new Condition{ Input = "Accept-Language", Pattern="en-US"}
        };

        List<Condition> usersEU = new List<Condition>
        {
            new Condition{ Input = "Accept-Language", Pattern="^(at|be|bg|hr|cy|cz|dk|ee|fi|fr|de|gr|hu|ie|it|lv|lt|lu|mt|nl|pl|pt|ro|sk|si|es|se)$"}
        };

        LimiterConfig config = new LimiterConfig() { Rules = new List<Rule>() };

        //add usersUSA
        config.Rules.Add(new Rule()
        {
            Name = "Rules targetting US based Users",
            Match = new Match { ApiUrl = targetResource },
            Conditions = usersUSA,
            Limits = new List<Limit>()
            {
                new Limit()
                {
                    Type = LimitType.TimeWindow,
                    WindowType = TimeWindowType.Second,
                    RequestLimit = 2 //we will allow 2 requests per second and test that below
                },
                new Limit()
                {
                    Type = LimitType.TimeWindow,
                    WindowType = TimeWindowType.Minute,
                    RequestLimit = 3 //we will allow 2 requests per second and test that below
                }

            }

        });

        //add usersEU
        config.Rules.Add(new Rule()
        {
            Name = "Rules targetting EU based Users",
            Match = new Match { ApiUrl = targetResource },
            Conditions = usersEU,
            Limits = new List<Limit>()
            {
                new Limit()
                {
                    Type = LimitType.RequestSpacing,
                    Spacing = TimeSpan.FromMilliseconds(500),
                }
            }

        });
        return config;
    }

    private LimitResult Run(string userToken, string apiEndpoint, LimiterConfig config, IDictionary<string, string>? userData = null)
    {
        IDictionary<string, string> headers = userData ?? new Dictionary<string, string>();

        LimitResult result = Limiter.Instance.ApplyRateLimit(userToken, apiEndpoint, headers, config);

        return result;
    }

}