using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Rules;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [Test]
    public async Task RequestsPerTimeSpanRule_Allows_Limited_Requests_In_TimeSpan()
    {
        // Arrange
        var rule = new RequestsPerTimeSpanRule(3, TimeSpan.FromSeconds(5)); // Allow 3 requests in 5 seconds
        const string TOKEN = "testToken";

        // Act
        var result1 = await rule.IsRequestAllowedAsync(TOKEN);
        var result2 = await rule.IsRequestAllowedAsync(TOKEN);
        var result3 = await rule.IsRequestAllowedAsync(TOKEN);
        var result4 = await rule.IsRequestAllowedAsync(TOKEN); // This should fail (4th request)

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
        Assert.IsFalse(result4); // 4th request should be blocked
    }

    [Test]
    public async Task TimeSinceLastCallRule_Allows_Request_After_Time_Elapsed()
    {
        // Arrange
        var rule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(2));
        const string TOKEN = "testToken";

        // Act
        var result1 = await rule.IsRequestAllowedAsync(TOKEN); // First request should succeed
        Thread.Sleep(1000); // Wait 1 second (less than allowed time)
        var result2 = await rule.IsRequestAllowedAsync(TOKEN); // Should fail
        Thread.Sleep(2000); // Wait additional 2 seconds
        var result3 = await rule.IsRequestAllowedAsync(TOKEN); // Should succeed after time elapsed

        // Assert
        Assert.IsTrue(result1);
        Assert.IsFalse(result2);
        Assert.IsTrue(result3);
    }

    [Test]
    public async Task CompositeRule_Allows_Request_Only_If_All_Rules_Are_Satisfied()
    {
        // Arrange
        var rule = new CompositeRule(new List<IRateLimitRule>
        {
            new RequestsPerTimeSpanRule(2, TimeSpan.FromSeconds(5)), // Allow 2 requests in 5 seconds
            new TimeSinceLastCallRule(TimeSpan.FromSeconds(1))       // Minimum 1 second between requests
        });

        const string TOKEN = "testToken";

        // Act
        Console.Write("Result 1: ");
        var result1 = await rule.IsRequestAllowedAsync(TOKEN); // Should succeed
        Thread.Sleep(500); // Wait 0.5 seconds
        Console.Write("Result 2: ");
        var result2 = await rule.IsRequestAllowedAsync(TOKEN); // Should fail (less than 1 second between requests)
        Thread.Sleep(1000); // Wait additional 1 second
        Console.Write("Result 3: ");
        var result3 = await rule.IsRequestAllowedAsync(TOKEN); // Should fail (3rd request in 5 seconds)

        // Assert
        Assert.IsTrue(result1);
        Assert.IsFalse(result2);
        Assert.IsFalse(result3);
    }

    [Test]
    public async Task RateLimiter_Manages_Multiple_Resources_Correctly()
    {
        // Arrange
        var rateLimiter = new RateLimiter();

        rateLimiter.ConfigureRateLimitRule("Resource1", new RequestsPerTimeSpanRule(3, TimeSpan.FromMinutes(1)));
        rateLimiter.ConfigureRateLimitRule("Resource2", new TimeSinceLastCallRule(TimeSpan.FromSeconds(2)));

        const string TOKEN1 = "user1";
        const string TOKEN2 = "user2";

        // Act & Assert for Resource1
        Assert.IsTrue((await rateLimiter.IsRequestAllowedAsync("Resource1", TOKEN1)).isAllowed);
        Assert.IsTrue((await rateLimiter.IsRequestAllowedAsync("Resource1", TOKEN1)).isAllowed);
        Assert.IsTrue((await rateLimiter.IsRequestAllowedAsync("Resource1", TOKEN1)).isAllowed);
        Assert.IsFalse((await rateLimiter.IsRequestAllowedAsync("Resource1", TOKEN1)).isAllowed); // Should fail on 4th request

        // Act & Assert for Resource2
        Assert.IsTrue((await rateLimiter.IsRequestAllowedAsync("Resource2", TOKEN2)).isAllowed);  // Should pass
        Thread.Sleep(1000); // Wait 1 second
        Assert.IsFalse((await rateLimiter.IsRequestAllowedAsync("Resource2", TOKEN2)).isAllowed); // Should fail (less than 2 seconds)
        Thread.Sleep(2000); // Wait additional 2 seconds
        Assert.IsTrue((await rateLimiter.IsRequestAllowedAsync("Resource2", TOKEN2)).isAllowed);  // Should pass
    }
    
    [Test]
    public async Task TimeSinceLastCallRule_Handles_Concurrent_Requests_Correctly()
    {
        // Arrange
        var rule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(2)); // Requires 2 seconds between requests
        const string TOKEN = "testToken";

        // Act: Send multiple requests concurrently
        var tasks = new List<Task<bool>>();

        // First batch: 5 concurrent requests at the same time
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(rule.IsRequestAllowedAsync(TOKEN));
        }

        var results1 = await Task.WhenAll(tasks); // Execute all requests concurrently

        // Delay to simulate some time passing, then another concurrent batch
        await Task.Delay(2500); // Wait 2.5 seconds, allowing the next request to succeed

        tasks.Clear(); // Reset the task list for the second batch

        // Second batch: 5 concurrent requests after waiting 2.5 seconds
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(rule.IsRequestAllowedAsync(TOKEN));
        }

        var results2 = await Task.WhenAll(tasks);

        // Assert
        Assert.IsTrue(results1[0], "The first request in the first batch should be allowed");
        Assert.IsFalse(results1[1], "The second request in the first batch should be blocked");
        Assert.IsFalse(results1[2], "The third request in the first batch should be blocked");
        Assert.IsFalse(results1[3], "The fourth request in the first batch should be blocked");
        Assert.IsFalse(results1[4], "The fifth request in the first batch should be blocked");

        Assert.IsTrue(results2[0], "The first request in the second batch should be allowed after the delay");
        Assert.IsFalse(results2[1], "The second request in the second batch should be blocked");
        Assert.IsFalse(results2[2], "The third request in the second batch should be blocked");
        Assert.IsFalse(results2[3], "The fourth request in the second batch should be blocked");
        Assert.IsFalse(results2[4], "The fifth request in the second batch should be blocked");
    }
    
    [Test]
    public async Task RequestsPerTimeSpanRule_Handles_Concurrent_Requests_Correctly()
    {
        // Arrange
        var rule = new RequestsPerTimeSpanRule(2, TimeSpan.FromSeconds(5)); // Allow 2 requests in 5 seconds
        const string TOKEN = "testToken";

        // Act: Send multiple requests concurrently
        var tasks = new List<Task<bool>>();

        // First batch: 5 concurrent requests at the same time
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(rule.IsRequestAllowedAsync(TOKEN));
        }

        var results1 = await Task.WhenAll(tasks); // Execute all requests concurrently

        // Delay to simulate some time passing, then another concurrent batch
        await Task.Delay(6000); // Wait 6 seconds, allowing the next requests to succeed

        tasks.Clear(); // Reset the task list for the second batch

        // Second batch: 5 concurrent requests after waiting 6 seconds
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(rule.IsRequestAllowedAsync(TOKEN));
        }

        var results2 = await Task.WhenAll(tasks);

        // Assert
        Assert.IsTrue(results1[0], "The first request in the first batch should be allowed");
        Assert.IsTrue(results1[1], "The second request in the first batch should be allowed");
        Assert.IsFalse(results1[2], "The third request in the first batch should be blocked");
        Assert.IsFalse(results1[3], "The fourth request in the first batch should be blocked");
        Assert.IsFalse(results1[4], "The fifth request in the first batch should be blocked");

        Assert.IsTrue(results2[0], "The first request in the second batch should be allowed after the delay");
        Assert.IsTrue(results2[1], "The second request in the second batch should be allowed after the delay");
        Assert.IsFalse(results2[2], "The third request in the second batch should be blocked");
        Assert.IsFalse(results2[3], "The fourth request in the second batch should be blocked");
        Assert.IsFalse(results2[4], "The fifth request in the second batch should be blocked");
    }
    
    [Test]
    public async Task RequestsPerTimeSpanRule_ShouldReturnNextAllowedTime_WhenLimitExceeded()
    {
        // Arrange
        var rule = new RequestsPerTimeSpanRule(3, TimeSpan.FromMinutes(1)); // Max 3 requests per minute
        const string TOKEN = "testToken";

        // Act
        // Make 3 allowed requests
        await rule.IsRequestAllowedAsync(TOKEN);
        await rule.IsRequestAllowedAsync(TOKEN);
        await rule.IsRequestAllowedAsync(TOKEN);

        // Fourth request should be denied
        var isAllowed = await rule.IsRequestAllowedAsync(TOKEN);
        var timeUntilReset = rule.GetTimeUntilReset(TOKEN);

        // Assert
        Assert.IsFalse(isAllowed, "The fourth request should be denied as the limit is exceeded.");
        Assert.Greater(timeUntilReset.TotalSeconds, 0, "Time until reset should be greater than zero.");
    }
    
    [Test]
    public async Task TimeSinceLastCallRule_ShouldReturnNextAllowedTime_WhenCalledTooSoon()
    {
        // Arrange
        var rule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(10)); // Minimum 10 seconds between calls
        const string TOKEN = "testToken";

        // Act
        // First request is allowed
        var firstAllowed = await rule.IsRequestAllowedAsync(TOKEN);

        // Second request immediately should be denied
        var secondAllowed = await rule.IsRequestAllowedAsync(TOKEN);
        var timeUntilReset = rule.GetTimeUntilReset(TOKEN);

        // Assert
        Assert.IsTrue(firstAllowed, "The first request should be allowed.");
        Assert.IsFalse(secondAllowed, "The second request should be denied because it's too soon.");
        Assert.Greater(timeUntilReset.TotalSeconds, 0, "Time until reset should be greater than zero.");
    }
    
    [Test]
    public async Task CompositeRule_ShouldReturnNextAllowedTime_WhenAnyRuleIsViolated()
    {
        // Arrange
        var rule1 = new RequestsPerTimeSpanRule(2, TimeSpan.FromMinutes(1)); // Max 2 requests per minute
        var rule2 = new TimeSinceLastCallRule(TimeSpan.FromSeconds(5)); // Minimum 5 seconds between calls
        var compositeRule = new CompositeRule(new List<IRateLimitRule> { rule1, rule2 });
        const string TOKEN = "user1";

        // Act
        // First request is allowed
        await compositeRule.IsRequestAllowedAsync(TOKEN);
        
        // Second request is also allowed
        await compositeRule.IsRequestAllowedAsync(TOKEN);
        
        // Third request should be denied due to the RequestsPerTimeSpanRule
        var isAllowed = await compositeRule.IsRequestAllowedAsync(TOKEN);
        var timeUntilReset = compositeRule.GetTimeUntilReset(TOKEN);

        // Assert
        Assert.IsFalse(isAllowed, "The third request should be denied by the composite rule.");
        Assert.Greater(timeUntilReset.TotalSeconds, 0, "Time until reset should be greater than zero.");
    }    
}
