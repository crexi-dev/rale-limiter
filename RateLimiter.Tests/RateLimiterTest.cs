using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using RateLimiter.Storage;
using RateLimiter.Interfaces;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private IRateLimiterStorage _memoryStorage;
    private RequestRateLimiterRule _rateLimiterRule;
    private TimeRateLimiterRule _timeLimiterRule;
    private TokenBucketRateLimiterRule _tokenBucketRateLimiterRule;
    private const int MaxRequests = 5;
    private static readonly TimeSpan TimeSpan = TimeSpan.FromMinutes(1);

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<IRateLimiterStorage>(provider =>
        {
            var storage = provider.GetService<IMemoryCache>();
            return new RateLimiterLocalStorage(storage);
        });

        var serviceProvider = services.BuildServiceProvider();

        _memoryStorage = serviceProvider.GetService<IRateLimiterStorage>();
    }

    [Test]
    public void OnActionExecuting_WhenRequestIsAllowed_SetsAccessToken()
    {
        var accessToken = "test-token";
        var cacheEntry = new CountBasedStorageEntry { Count = 0, LastAccessTime = DateTime.UtcNow };
        _memoryStorage.Set(accessToken, cacheEntry);
        _rateLimiterRule = new RequestRateLimiterRule(_memoryStorage, MaxRequests, TimeSpan);
        var result = _rateLimiterRule.Execute(accessToken);
        Assert.IsTrue(result.Item1);
    }

    [Test]
    public void OnActionExecuting_WhenRateLimitExceeded_ReturnsTooManyRequestsResult()
    {
        var accessToken = "test-token";

        var cacheEntry = new CountBasedStorageEntry { Count = MaxRequests, LastAccessTime = DateTime.UtcNow };
        _memoryStorage.Set($"{nameof(RequestRateLimiterRule)}_{accessToken}", cacheEntry);
        _rateLimiterRule = new RequestRateLimiterRule(_memoryStorage, MaxRequests, TimeSpan);
        var result = _rateLimiterRule.Execute(accessToken);
        Assert.IsFalse(result.Item1);
    }

    [Test]
    public void IsRequestAllowed_WhenCalled_TracksRequestCount()
    {
        var accessToken = "test-token";
        var cacheEntry = new CountBasedStorageEntry { Count = 0, LastAccessTime = DateTime.UtcNow };
        _memoryStorage.Set($"{nameof(RequestRateLimiterRule)}_{accessToken}", cacheEntry);
        _rateLimiterRule = new RequestRateLimiterRule(_memoryStorage, MaxRequests, TimeSpan)
        {
            AccessToken = accessToken
        };

        var result = _rateLimiterRule.IsRequestAllowed();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, cacheEntry.Count);
    }

    [Test]
    public void IsRequestAllowed_WhenRateLimitExceeded_ReturnsSuccessFalse()
    {
        var accessToken = "test-token";
        var cacheEntry = new CountBasedStorageEntry { Count = MaxRequests, LastAccessTime = DateTime.UtcNow };
        _memoryStorage.Set($"{nameof(RequestRateLimiterRule)}_{accessToken}", cacheEntry);
        _rateLimiterRule = new RequestRateLimiterRule(_memoryStorage, MaxRequests, TimeSpan)
        {
            AccessToken = accessToken
        };
        var result = _rateLimiterRule.IsRequestAllowed();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(cacheEntry, result.StorageEntry);
    }

    [Test]
    public void OnActionExecuting_WhenRateLimitResetsAfterWait_AllowsRequestAfterTimeout()
    {
        var accessToken = "test-token";
        _rateLimiterRule = new RequestRateLimiterRule(_memoryStorage, 2, TimeSpan.FromSeconds(5)); // 2 requests every 5 seconds
        var result = _rateLimiterRule.Execute(accessToken);
        Assert.IsTrue(result.Item1);
        result = _rateLimiterRule.Execute(accessToken);
        Assert.IsTrue(result.Item1);
        // Simulate hitting the rate limit on the next request
        result = _rateLimiterRule.Execute(accessToken);
        // Assert rate limit exceeded
        Assert.IsFalse(result.Item1);
        ParseAndWait(result);
        //// After waiting, make a new request (this should succeed)
        result = _rateLimiterRule.Execute(accessToken);
        //// Assert no rate limit error after wait
        Assert.IsTrue(result.Item1);
    }

    [Test]
    public void OnActionExecuting_WhenRateLimitResetsAfterWait_AllowsRequestAfterTimeout_2()
    {
        var accessToken = "test-token";
        _timeLimiterRule = new TimeRateLimiterRule(_memoryStorage, TimeSpan.FromSeconds(5)); // 1 request every 5 seconds
        // Simulate first valid request (should succeed)
        var result = _timeLimiterRule.Execute(accessToken);
        // Assert no rate limit exceeded
        Assert.IsTrue(result.Item1);
        // Simulate hitting the rate limit on the next request
        result = _timeLimiterRule.Execute(accessToken);
        // Assert rate limit exceeded
        Assert.IsFalse (result.Item1);
        ParseAndWait(result);
        // After waiting, make a new request (this should succeed)
        result = _timeLimiterRule.Execute(accessToken);
        // Assert no rate limit error after wait
        Assert.IsTrue(result.Item1);
    }

    [Test]
    public void OnActionExecuting_WhenTokenRuleRefillsFasterThanTheRequests()
    {
        // Arrange
        var accessToken = "test-token";
        _tokenBucketRateLimiterRule = new TokenBucketRateLimiterRule(_memoryStorage, 10, 3, TimeSpan.FromMilliseconds(0.01)); 
        //10 max requests, but the requests refill at a rate of a 3 request every 0.01 of a millisecond. so doing 13 should be fine

        //exhaust the tokens
        for(var i = 0; i < 13; i++)
        {
            var result = _tokenBucketRateLimiterRule.Execute(accessToken);
            Assert.IsTrue(result.Item1);
        }
    }

    [Test]
    public void OnActionExecuting_UsingTheRegionalDelegator()
    {
        var context = GetExecutingContext();

        var usToken = "US-Token";
        var euToken = "EU-Token";

        RegionBasedRateLimiterDelegator? regionDelegator = null;
        for (var i = 0; i < 60; i++)
        {
            if (i % 20 == 0 || regionDelegator == null)
            {
                var randomRateLimiterRule = GetRandomRateLimiterRule();
                var randomTimeLimiterRule = GetDifferentTimeLimiterRule(randomRateLimiterRule);

                regionDelegator = new RegionBasedRateLimiterDelegator(
                    usRateLimiter: randomRateLimiterRule,
                    otherRateLimiter: randomTimeLimiterRule
                );
            }

            if (i % 2 == 0)
            {
                //eu request
                context.HttpContext.Request.Headers["X-Access-Token"] = euToken;
            } 
            else
            {
                //us request
                context.HttpContext.Request.Headers["X-Access-Token"] = usToken;
            }

            regionDelegator.OnActionExecuting(context);

            if (context.Result != null)
            {
                //if error wait before sending more requests
                ParseAndWait(context);
            }

            context.Result = null;
        }
    }

    /// <summary>
    /// Parses the retryafter header and waits
    /// </summary>
    /// <param name="context"></param>
    private static void ParseAndWait(ActionExecutingContext context)
    {
        var rateLimitResult = context.Result as ContentResult;
        Assert.AreEqual((int)HttpStatusCode.TooManyRequests, rateLimitResult.StatusCode);
        Assert.AreEqual("Rate limit exceeded.", rateLimitResult.Content);

        // Get Retry-After header value and sleep
        var retryAfterHeader = context.HttpContext.Response.Headers["Retry-After"];
        Assert.IsNotEmpty(retryAfterHeader);
        var retryAfterSeconds = int.Parse(retryAfterHeader);

        // Simulate waiting until the rate limit window expires
        Thread.Sleep(retryAfterSeconds * 1000); // Sleep for the amount of time in Retry-After
    }

    private static void ParseAndWait(Tuple<bool, double> result)
    {
        Thread.Sleep((int)result.Item2 * 1500); // Sleepin a little longer because to allow the cache to cleanup
    }

    private IRateLimiterRule GetRandomRateLimiterRule()
    {
        // Randomly choose between different rate limiter rules
        var random = new Random();
        var ruleChoice = random.Next(0, 3);

        switch (ruleChoice)
        {
            case 0:
                return new RequestRateLimiterRule(_memoryStorage, 2, TimeSpan.FromSeconds(5)); // Example: 2 requests every 5 seconds
            case 1:
                return new TimeRateLimiterRule(_memoryStorage, TimeSpan.FromSeconds(5)); // Example: 1 request every 5 seconds
            default:
                return new TokenBucketRateLimiterRule(_memoryStorage, 10, 3, TimeSpan.FromMilliseconds(0.01)); // Example: 10 tokens, refilling 3 tokens per 0.01 milliseconds
        }
    }

    private IRateLimiterRule GetDifferentTimeLimiterRule(IRateLimiterRule selectedRule)
    {
        // Randomly choose a different rule that is not the same type as the selectedRule
        var random = new Random();
        IRateLimiterRule newRule;

        do
        {
            var ruleChoice = random.Next(0, 3); // Now choose from 3 different options

            switch (ruleChoice)
            {
                case 0:
                    newRule = new RequestRateLimiterRule(_memoryStorage, 2, TimeSpan.FromSeconds(5)); // Example: 2 requests every 5 seconds
                    break;
                case 1:
                    newRule = new TimeRateLimiterRule(_memoryStorage, TimeSpan.FromSeconds(5)); // Example: 1 request every 5 seconds
                    break;
                default:
                    newRule = new TokenBucketRateLimiterRule(_memoryStorage, 10, 3, TimeSpan.FromMilliseconds(0.01)); // Example: 10 tokens, refilling 3 tokens per 0.01 milliseconds
                    break;
            }

        } while (newRule.GetType() == selectedRule.GetType()); // Ensure it's not the same type

        return newRule;
    }

    private static ActionExecutingContext GetExecutingContext(bool addToken = false)
    {
        var httpContext = new DefaultHttpContext();

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        var controller = new Mock<ControllerBase>();
        var context = new ActionExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller.Object
        );

        if(addToken)
        {
            var accessToken = "test-token";
            httpContext.Request.Headers.Add("X-Access-Token", accessToken);
        }

        return context;
    }
}