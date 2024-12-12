using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Repositories;
using RateLimiter.Enums;
using System.Threading;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private IClient _client;
    private IRateLimitingRule _ruleXRequestsWithinTime;
    private IRateLimitingRule _ruleTimePassedSinceLastCall;
    private IResourceRateLimit _resourceRateLimit;

    private const string _resource1 = "Resource1";
    private const string _resource2 = "Resource2";
    private const string _resource3 = "Resource3";
    private const string _regionUS = "US";
    private const string _regionEU = "EU";
    private const string _regionASIA = "ASIA";
    private const string _ruleNameXRequestsWithinTime = "RuleXRequestsPerTimespan";
    private const string _ruleNameRuleTimePassedSinceLastCall = "RuleTimePassedSinceLastCall";

    [SetUp]
	public void Setup()
	{
        _resourceRateLimit = new ResourceRateLimit();

        //2 requests are allowed per every 3 seconds
        _ruleXRequestsWithinTime = new RuleXRequestsPerTimespan(2, new System.TimeSpan(0, 0, 3));

        //A request is allowed if the previous successful request was at least 1 second in the past
        _ruleTimePassedSinceLastCall = new RuleTimePassedSinceLastCall(new System.TimeSpan(0, 0, 1));

        /*  Configure resource1 with rule RuleXRequestsPerTimespan for US region
            Configure resource2 with rule RuleTimePassedSinceLastCall for EU region
            Configure resource3 with both rules for ASIA region
        */
        _resourceRateLimit.AddRuleResource(_resource1 + "." + _regionUS, _ruleXRequestsWithinTime);
        _resourceRateLimit.AddRuleResource(_resource2 + "." + _regionEU, _ruleTimePassedSinceLastCall);
        _resourceRateLimit.AddRuleResource(_resource3 + "." + _regionASIA, _ruleXRequestsWithinTime);
        _resourceRateLimit.AddRuleResource(_resource3 + "." + _regionASIA, _ruleTimePassedSinceLastCall);
    }

    [Test]
    public void Run_RuleTimePassedSinceLastCall_Returns_Success()
    {
        //Arrange
        _client = new Client("EU.xxxx");
        _client.AddRequest();

        //Act
        Thread.Sleep(1000); //Allow 1 second to pass before checking for the last recorded time
        var result = _resourceRateLimit.CheckRule(_resource2, _client);

        //Assert
        Assert.IsTrue(result?.IsSuccess);
    }

    [Test]
    public void Run_RuleTimePassedSinceLastCall_Returns_Failure()
    {
        //Arrange
        _client = new Client("EU.xxxx");
        _client.AddRequest();

        //Act
        var result = _resourceRateLimit.CheckRule(_resource2, _client);

        //Assert
        Assert.IsFalse(result?.IsSuccess);
    }

    public void Check_Clients_Region_Returns_Success()
    {
        //Assert
        Assert.AreEqual(Region.None, (new Client("FAKE.xxxx")).ReturnRegion());

        Assert.AreEqual(Region.US, (new Client("US.xxxx")).ReturnRegion());

        Assert.AreEqual(Region.EU, (new Client("EU.xxxx")).ReturnRegion());

        Assert.AreEqual(Region.ASIA, (new Client("ASIA.xxxx")).ReturnRegion());
    }

    
    [Test]
	public void Run_RuleXRequestsWithinTime_Returns_Success()
	{
        //Arrange
        _client = new Client("US.xxxx");
        _client.AddRequest();
        _client.AddRequest();

        //Act
        Thread.Sleep(3000);
        var result = _resourceRateLimit.CheckRule(_resource1, _client);

		//Assert
		Assert.IsTrue(result?.IsSuccess);
    }

    
    [Test]
    public void Run_RuleXRequestsWithinTime_Returns_Failure()
    {
        //Arrange
        _client = new Client("US.xxxx");
        _client.AddRequest();
        _client.AddRequest();

        //Act
        var result = _resourceRateLimit.CheckRule(_resource1, _client);

        //Assert
        Assert.IsFalse(result?.IsSuccess);
    }

    
    [Test]
    public void Set_RuleXRequestsWithinTime_MaxRequest_ToDefault_Returns_One()
    {
        //Arrange
        IRateLimitingRule rule = new RuleXRequestsPerTimespan(0, new System.TimeSpan(0, 0, 0));

        //Assert
        Assert.AreEqual(1, rule.ReturnMaxRequests());
    }

    [Test]
    public void Get_DoNotExistResource_Returns_Null()
    {
        //Act
        _client = new Client("ASIA.xxxx");

        //Arrange
        //Client is not configured with Resource1
        var result = _resourceRateLimit.CheckRule(_resource1, _client);

        //Assert
        Assert.IsNull(result);
    }

    [Test]
    public void Run_MultiRules_Break_RuleXRequestsPerTimespan_Returns_Failure()
    {
        //Act
        _client = new Client("ASIA.xxxx");

        _client.AddRequest();

        Thread.Sleep(1000);

        _client.AddRequest();

        Thread.Sleep(1000);

        //Arrange
        var result = _resourceRateLimit.CheckRule(_resource3, _client);

        //Assert
        Assert.IsFalse(result?.IsSuccess);

        Assert.AreEqual(_ruleNameXRequestsWithinTime, result?.RuleName);
    }

    
    [Test]
    public void Run_MultiRules_Break_RuleTimePassedSinceLastCall_Returns_Failure()
    {
        //Act
        _client = new Client("ASIA.xxxx");

        _client.AddRequest();

        //Arrange
        var result = _resourceRateLimit.CheckRule(_resource3, _client);

        //Assert
        Assert.IsFalse(result.IsSuccess);

        Assert.AreEqual(_ruleNameRuleTimePassedSinceLastCall, result.RuleName);
    }
}