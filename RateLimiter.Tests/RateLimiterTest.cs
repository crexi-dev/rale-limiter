using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void Example()
	{
		Assert.That(true, Is.True);
	}

    #region Multiple Rules
    [Test]
	public async Task BothRateLimitRule_Test1()
	{
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1)),
            new TimeSpanRule(timeSpan: TimeSpan.FromMinutes(5))
        };
        
        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";

        bool isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
        Assert.IsTrue(isAllowed);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
        Assert.IsFalse(isAllowed);
    }

    [Test]
    public async Task BothRateLimitRule_Test2()
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1)), // Only 5 per minute, and only once per 5 seconds
            new TimeSpanRule(timeSpan: TimeSpan.FromSeconds(5))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";

        bool isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
        Assert.IsTrue(isAllowed);
         
        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US); // not allowed by rule 2
        Assert.IsFalse(isAllowed);

        for (int i = 0; i < 4; i++)
        {
            Console.WriteLine(i);
            Thread.Sleep(5000);
            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
            Assert.IsTrue(isAllowed);
        }

        Thread.Sleep(5000);
        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US); // not allowed by rule 1
        Assert.IsFalse(isAllowed);
    }

    [Test]
    public async Task BothRateLimitRule_Test3()
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1)), // Only 5 per minute, and only once per 5 seconds
            new TimeSpanRule(timeSpan: TimeSpan.FromSeconds(5))
        };

        var _rateLimiter = new RateLimiter(rules);

        string[] accessTokens = new string[5];
        string baseAccessToken = "AccessToken";
        bool isAllowed;

        for (int i = 0; i < 5; i++)
        {
            accessTokens[i] = baseAccessToken + i.ToString();
            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessTokens[i], DateTime.UtcNow);
            Assert.IsTrue(isAllowed);

            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessTokens[i], DateTime.UtcNow); // rejected by rule 2, no sleep
            Assert.IsFalse(isAllowed);
        }

        Thread.Sleep(5000);

        // All tokens ran once - 4 access remaining per token, 5th iteration should fail
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (i == 4)
                {
                    isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessTokens[j], DateTime.UtcNow); // rejected by rule 1, token has been placed 5 times
                    Assert.IsFalse(isAllowed);
                }
                else
                {
                    isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessTokens[j], DateTime.UtcNow);
                    Assert.IsTrue(isAllowed);
                }
            }

            Thread.Sleep(5000);
        }
    }

    [Test]
    public async Task RegionRule_RequestsPerTimeSpanRule_Test1()
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1)),
            new RegionRateLimitRule(US_maxRequests: 5, US_timeSpan: TimeSpan.FromMinutes(1), EU_timeSpan: TimeSpan.FromSeconds(5))
        };

        // Passing region as EU will apply the TimeSpanRule (by region) and also the RequestsPerTimeSpanRule

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";
        bool isAllowed;

        for (int i = 0; i < 5; i++)
        {
            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
            Assert.IsTrue(isAllowed);
            Thread.Sleep(5000);
        }

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
        Assert.IsFalse(isAllowed);
    }

    [Test]
    public async Task RegionRule_RequestsPerTimeSpanRule_Test2()
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1)),
            new RegionRateLimitRule(US_maxRequests: 5, US_timeSpan: TimeSpan.FromMinutes(1), EU_timeSpan: TimeSpan.FromSeconds(10))
        };

        // Not passing region or passing region as ALL_REGIONS will throw exception

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow));

        Assert.NotNull(ex);
        Assert.That(ex.Message, Is.EqualTo("Invalid region specified"));

        ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.ALL_REGIONS));

        Assert.NotNull(ex);
        Assert.That(ex.Message, Is.EqualTo("Invalid region specified"));
    }
    #endregion

    #region RequestsPerTimeSpanRule
    [Test]
    public async Task RequestsPerTimeSpanRule_Test1()
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";

        for (int i = 0; i < 10; i++)
        {
            bool isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
            if (i < 5)
            {
                Assert.IsTrue(isAllowed);
            }
            else
            {
                Assert.IsFalse(isAllowed);
            }
        }
    }

    [Test]
    public async Task RequestsPerTimeSpanRule_Test2()
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromMinutes(1))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";
        bool isAllowed;

        for (int i = 0; i < 5; i++)
        {
            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
            Assert.IsTrue(isAllowed);
        }

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
        Assert.IsFalse(isAllowed);

        Thread.Sleep(60000);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
        Assert.IsTrue(isAllowed);
    }

    [Test]
    public async Task RequestsPerTimeSpanRule_Test3() // Many accessTokens
    {
        var rules = new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(maxRequests: 5, timeSpan: TimeSpan.FromSeconds(10))
        };

        var _rateLimiter = new RateLimiter(rules);

        string baseAccessToken = "AccessToken";
        bool isAllowed;

        for (int i = 0; i < 10; i++)
        {
            string accessToken = baseAccessToken + i.ToString();
            for (int j = 0; j < 5; j++) // All 10 users run 5 times
            {
                isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
                Assert.IsTrue(isAllowed);
            }
        }

        isAllowed = await _rateLimiter.IsRequestAllowedAsync("AccessToken0", DateTime.UtcNow); // User 0 6th request should fail
        Assert.IsFalse(isAllowed);

        Thread.Sleep(10000);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync("AccessToken1", DateTime.UtcNow); // timespan has passed
        Assert.IsTrue(isAllowed);
    }
    #endregion

    #region TimeSpanRule
    [Test]
    public async Task TimeSpanRule_Test1()
    {
        var rules = new List<IRateLimitRule>
        {
            new TimeSpanRule(timeSpan: TimeSpan.FromMinutes(1))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";
        bool isAllowed;

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
        Assert.IsTrue(isAllowed);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
        Assert.IsFalse(isAllowed);
    }

    [Test]
    public async Task TimeSpanRule_Test2()
    {
        var rules = new List<IRateLimitRule>
        {
            new TimeSpanRule(timeSpan: TimeSpan.FromMinutes(1))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";
        bool isAllowed;

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
        Assert.IsTrue(isAllowed);

        Thread.Sleep(60000);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
        Assert.IsTrue(isAllowed);
    }

    [Test]
    public async Task TimeSpanRule_Test3() // Many accessTokens
    {
        var rules = new List<IRateLimitRule>
        {
            new TimeSpanRule(timeSpan: TimeSpan.FromSeconds(10))
        };

        var _rateLimiter = new RateLimiter(rules);
        
        string baseAccessToken = "AccessToken";
        bool isAllowed;

        for (int i = 0; i < 10; i++)
        {
            string accessToken = baseAccessToken + i.ToString();
            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow);
            Assert.IsTrue(isAllowed);
        }

        isAllowed = await _rateLimiter.IsRequestAllowedAsync("AccessToken0", DateTime.UtcNow);
        Assert.IsFalse(isAllowed);

        Thread.Sleep(10000);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync("AccessToken1", DateTime.UtcNow);
        Assert.IsTrue(isAllowed);
    }
    #endregion

    #region RegionRule
    [Test] 
    public async Task RegionRule_Test1()
    {
        var rules = new List<IRateLimitRule>
        {
            new RegionRateLimitRule(US_maxRequests: 5, US_timeSpan: TimeSpan.FromMinutes(1), EU_timeSpan: TimeSpan.FromSeconds(10))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";
        bool isAllowed;

        for (int i = 0; i < 5; i++)
        {
            isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
            Assert.IsTrue(isAllowed);
        }

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.US);
        Assert.IsFalse(isAllowed);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.EU);
        Assert.IsTrue(isAllowed);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.EU);
        Assert.IsFalse(isAllowed);

        Thread.Sleep(10000);

        isAllowed = await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.EU);
        Assert.IsTrue(isAllowed);
    }

    [Test] 
    public async Task RegionRule_Test2() // Passing ALL_REGIONS throws exception
    {
        var rules = new List<IRateLimitRule>
        {
            new RegionRateLimitRule(US_maxRequests: 5, US_timeSpan: TimeSpan.FromMinutes(1), EU_timeSpan: TimeSpan.FromSeconds(10))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";
        
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow, Region.ALL_REGIONS));

        Assert.NotNull(ex);
        Assert.That(ex.Message, Is.EqualTo("Invalid region specified"));
    }

    [Test] 
    public async Task RegionRule_Test3() // Not passing region throws exception
    {
        var rules = new List<IRateLimitRule>
        {
            new RegionRateLimitRule(US_maxRequests: 5, US_timeSpan: TimeSpan.FromMinutes(1), EU_timeSpan: TimeSpan.FromSeconds(10))
        };

        var _rateLimiter = new RateLimiter(rules);

        string accessToken = "AccessToken";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rateLimiter.IsRequestAllowedAsync(accessToken, DateTime.UtcNow));

        Assert.NotNull(ex);
        Assert.That(ex.Message, Is.EqualTo("Invalid region specified"));
    }
    #endregion
}