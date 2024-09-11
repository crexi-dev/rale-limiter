using RateLimiter.Rules;
using FluentAssertions;
using System.Collections.Concurrent;

namespace RateLimiter.Tests;

public class RequestCountRuleTests
{
    [Fact]
    public async Task Should_Allows_Requests_Within_Limit()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 3, windowSize: TimeSpan.FromMinutes(1));
        var clientId = "client1";
        var resource = "resource1";

        // Act
        var result1 = await rule.IsRequestAllowed(clientId, resource);
        var result2 = await rule.IsRequestAllowed(clientId, resource);
        var result3 = await rule.IsRequestAllowed(clientId, resource);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Deny_Requests_Exceeding_Limit()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 2, windowSize: TimeSpan.FromMinutes(1));
        var clientId = "client1";
        var resource = "resource1";

        // Act
        await rule.IsRequestAllowed(clientId, resource); // 1st request
        await rule.IsRequestAllowed(clientId, resource); // 2nd request
        var result = await rule.IsRequestAllowed(clientId, resource); // 3rd request

        // Assert
        result.Should().BeFalse(); // Should be denied
    }

    [Fact]
    public async Task Should_Allow_Request_After_Window_Expires()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 2, windowSize: TimeSpan.FromSeconds(3));
        var clientId = "client1";
        var resource = "resource1";

        // Act
        await rule.IsRequestAllowed(clientId, resource); // 1st request
        await rule.IsRequestAllowed(clientId, resource); // 2nd request
        Task.Delay(3100).Wait(); // Wait more than window size
        var result = await rule.IsRequestAllowed(clientId, resource); // Request after window expires

        // Assert
        result.Should().BeTrue(); // Should be allowed
    }

    [Fact]
    public void RequestCountRule_Should_Handle_Negative_MaxRequests_Parameter()
    {
        // Arrange
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RequestCountRule(maxRequests: -1, windowSize: TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void RequestCountRule_Should_Handle_Negative_WindowSize_Parameter()
    {
        // Arrange
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RequestCountRule(maxRequests: 1, windowSize: TimeSpan.FromSeconds(-1)));
    }

   [Fact]
    public async Task IsRequestAllowed_Should_Handle_Empty_ClientId()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 1, windowSize: TimeSpan.FromSeconds(1)); // Negative maxRequests
        var clientId = "";
        var resource = "resource1";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await rule.IsRequestAllowed(clientId, resource));
    }

    [Fact]
    public async Task IsRequestAllowed_Should_Handle_Null_ClientId()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 1, windowSize: TimeSpan.FromSeconds(1)); // Negative maxRequests
        string clientId = null!;
        var resource = "resource1";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await rule.IsRequestAllowed(clientId, resource));
    }

    [Fact]
    public async Task IsRequestAllowed_Should_Handle_Empty_Resource()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 1, windowSize: TimeSpan.FromSeconds(1)); // Negative maxRequests
        var clientId = "client1";
        var resource = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await rule.IsRequestAllowed(clientId, resource));
    }

    [Fact]
    public async Task IsRequestAllowed_Should_Handle_Null_Resource()
    {
        // Arrange
        var rule = new RequestCountRule(maxRequests: 1, windowSize: TimeSpan.FromSeconds(1)); // Negative maxRequests
        var clientId = "client1";
        string resource = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await rule.IsRequestAllowed(clientId, resource));
    }

    #region Concurrency Tests

    [Fact]
    public async Task Should_Handle_Concurrency_Correctly()
    {
        // Arrange
        const int maxConcurrency = 100;
        var rule = new RequestCountRule(maxRequests: 10, windowSize: TimeSpan.FromSeconds(1));
        var clientId = "concurrentClient";
        var resource = "resource1";
        var results = new ConcurrentBag<Task<bool>>();

        // Act
        var tasks = Enumerable.Range(0, maxConcurrency).Select(_ => Task.Run(async () =>
        {
            var isAllowed = await rule.IsRequestAllowed(clientId, resource);
            results.Add(Task.FromResult(isAllowed));
        }));

        await Task.WhenAll(tasks);
        // Await all tasks within the ConcurrentBag to get the boolean results
        var completedResults = await Task.WhenAll(results);

        // Assert
        // Only 10 should be allowed; the rest should be denied.
        var allowedCount = completedResults.Count(x => x);
        var deniedCount = completedResults.Count(x => !x);

        allowedCount.Should().Be(10);
        deniedCount.Should().Be(maxConcurrency - 10);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async void RequestCountRule_Should_Perform_Under_Heavy_Load()
    {
        // Arrange
        const int maxRequests = 10000;
        var rule = new RequestCountRule(maxRequests: maxRequests, windowSize: TimeSpan.FromSeconds(10));
        var clientId = "performanceClient";
        var resource = "resource1";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var tasks = Enumerable.Range(0, maxRequests).Select(_ => rule.IsRequestAllowed(clientId, resource));
        await Task.WhenAll(tasks);

        stopwatch.Stop();

        // Assert
        // Expect the operation to complete quickly, within a reasonable performance window. Need to revise this value as we add more code.
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000);
    }
    #endregion
}
