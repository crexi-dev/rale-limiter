using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Models.Entities;
using RateLimiter.Models.RatePolicies;
using RateLimiter.Repositories;
using RateLimiter.Services;
using RateLimiter.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	private Mock<IPersistentProvider> _persistentProviderMock;
	private Mock<IPolicyVerifier> _policyVerifierMock;
	private IUserRepository _userRepository;
	private IResourceRepository _resourceRepository;
	private IPolicyRepository _policyRepository;
	private IPolicyService _policyService;
	private RateLimitRequestValidator _validator;
    private List<UserActivity> _activities;
    private int _minLastCallSeconds;
    private int _maxCallsInTimeSpan;

	[SetUp]
	public void Setup()
	{
		_persistentProviderMock = new Mock<IPersistentProvider>();
		_policyVerifierMock = new Mock<IPolicyVerifier>();

		_userRepository = new UserRepository(_persistentProviderMock.Object);
		_resourceRepository = new ResourceRepository(_persistentProviderMock.Object);
		_policyRepository = new PolicyRepository(_persistentProviderMock.Object);

		_policyService = new PolicyService(_policyRepository, _policyVerifierMock.Object);

		_validator = new RateLimitRequestValidator();

        _activities = new List<UserActivity>();
        SetupUserActivity();

        _minLastCallSeconds = 5;
        _maxCallsInTimeSpan = 5;
    }

	[Test]
	public async Task RateLimitService_Invalid_Parameters_Test()
	{
		var rateLimitService = new RateLimitService(_userRepository, _resourceRepository, _policyService, _validator);
		var result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
		{
		});
		Assert.AreEqual(false, result.Success);
		Assert.AreEqual(2, result.Errors.Count);
	}

    [Test]
    public async Task RateLimitService_UserOrResource_NotFound_Test()
    {
		// setup one user and one resource
		var userId = "user1";
		var resourceId = "resource1";
		SetupUserAndResource(userId, resourceId);

		// check rate limit for another user to access the resource
        var rateLimitService = new RateLimitService(_userRepository, _resourceRepository, _policyService, _validator);
        var result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
        {
			UserId = "user2",
			ResourceId = resourceId
        });
        Assert.AreEqual(false, result.Success);
        Assert.AreEqual(1, result.Errors.Count);

        // check rate limit for the user to access another resource
        result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
        {
            UserId = userId,
            ResourceId = "resource2"
        });
        Assert.AreEqual(false, result.Success);
        Assert.AreEqual(1, result.Errors.Count);
    }

    [TestCase(PolicyType.NoLimit)]
    [TestCase(PolicyType.TimeSpan)]
    [TestCase(PolicyType.LastCall)]
    public async Task RateLimitService_Valid_UserAndResource_Test(PolicyType policyType)
    {
        // setup one user and one resource
        var userId = "user1";
        var resourceId = "resource1";
        SetupUserAndResource(userId, resourceId);

        SetupUserResourcePolicy(userId, resourceId, policyType);

        // check rate limit for the first call, all shoule be success
        var rateLimitService = new RateLimitService(_userRepository, _resourceRepository, _policyService, _validator);
        var result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
        {
            UserId = userId,
            ResourceId = resourceId
        });
        Assert.AreEqual(true, result.Success);
    }

    [Test]
    public async Task RateLimitService_LastCallPolicy_Test()
    {
        // setup one user and one resource
        var userId = "user1";
        var resourceId = "resource1";
        SetupUserAndResource(userId, resourceId);

        SetupUserResourcePolicy(userId, resourceId, PolicyType.LastCall);

        // the first call should be successful
        var rateLimitService = new RateLimitService(_userRepository, _resourceRepository, _policyService, _validator);
        var result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
        {
            UserId = userId,
            ResourceId = resourceId
        });
        Assert.AreEqual(true, result.Success);

        // the second call shold fail
        result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
        {
            UserId = userId,
            ResourceId = resourceId
        });
        Assert.AreEqual(false, result.Success);

        // sleep a while and the third call shold be successful
        Thread.Sleep(_minLastCallSeconds * 1000);
        result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
        {
            UserId = userId,
            ResourceId = resourceId
        });
        Assert.AreEqual(true, result.Success);
    }

    [Test]
    public async Task RateLimitService_TImeSpanPolicy_Test()
    {
        // setup one user and one resource
        var userId = "user1";
        var resourceId = "resource1";
        SetupUserAndResource(userId, resourceId);

        SetupUserResourcePolicy(userId, resourceId, PolicyType.TimeSpan);

        // the first _maxCallsInTimeSpan calls should be successful
        var rateLimitService = new RateLimitService(_userRepository, _resourceRepository, _policyService, _validator);

        var totalCalls = _maxCallsInTimeSpan + 1;
        for (int i = 0; i < totalCalls; i++)
        {
            var result = await rateLimitService.CheckRateLimitAsync(new Models.Apis.RateLimitRequest()
            {
                UserId = userId,
                ResourceId = resourceId
            });
            if (i == _maxCallsInTimeSpan)
            {
                Assert.AreEqual(false, result.Success);
            }
            else
            {
                Assert.AreEqual(true, result.Success);
            }
        }
    }

    private void SetupUserActivity()
    {
        _persistentProviderMock.Setup(x => x.AddUserAccessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback((string userId, string resourceId, DateTime dt) => 
            {
                _activities.Add(new UserActivity()
                {
                    UserId = userId,
                    ResourceId = resourceId,
                    AccessTime = dt,
                });
            });
        _policyVerifierMock.Setup(x => x.GetAccessCountInSecondsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string userId, string resourceId, int seconds) =>
            {
                return _activities.Where(x => DateTime.UtcNow.AddSeconds(-seconds) < x.AccessTime).Count();
            });
    }

    private void SetupUserAndResource(string userId, string resourceId)
	{
        _persistentProviderMock.Setup(x => x.GetUserAsync(userId)).ReturnsAsync(new Models.Entities.User()
        {
            UserId = userId,
            DisplayName = $"Name for {userId}",
        });
        _persistentProviderMock.Setup(x => x.GetResourceAsync(resourceId)).ReturnsAsync(new Models.Entities.Resource()
        {
            ResourceId = resourceId,
            Content = $"Content for {resourceId}"
        });
    }

    private void SetupUserResourcePolicy(string userId, string resourceId, PolicyType policyType)
    {
        var policyJson = string.Empty;
        switch (policyType)
        {
            case PolicyType.NoLimit:
                policyJson = "{}";
                break;
            case PolicyType.TimeSpan:
                policyJson = JsonConvert.SerializeObject(new TimeSpanInfo()
                {
                    MaxCalls = _maxCallsInTimeSpan,
                    SpanInSeconds = 60,
                });
                break;
            case PolicyType.LastCall:
                policyJson = JsonConvert.SerializeObject(new LastCallInfo()
                {
                    MinLastCallSeconds = _minLastCallSeconds,
                });
                break;
        }

        _persistentProviderMock.Setup(x => x.GetPolicyAsync(It.IsAny<string>()))
            .ReturnsAsync(new Models.Entities.Policy()
            {
                PolicyId = "p1",
                PolicyName = policyType.ToString(),
                PolicyJson = policyJson,
            });
        _persistentProviderMock.Setup(x => x.GetUserResourcePoliciesAsync(userId, resourceId))
            .ReturnsAsync(new System.Collections.Generic.List<Models.Entities.Policy>()
            {
                new Models.Entities.Policy()
                {
                    PolicyId = "p1",
                    PolicyName = policyType.ToString(),
                    PolicyJson = policyJson,
                }
            });
    }
}