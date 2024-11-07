using NUnit.Framework;
using RateLimiter.CustomLimiters;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

/// <summary>
/// This class provides basic tests on the custom random rate limiter.
/// The random rate limiter was created for demonstration purposes only in order 
/// to demonstrate how to create and utilize a custom rate limiter in this solution.
/// It should not be used in production code, and therefore these tests provide minimal coverage.
/// </summary>
[TestFixture]
public class RandomRateLimiterTests
{
    private RandomRateLimiter _randomRateLimiter;

    [SetUp]
    public void SetUp()
    {
        // Initialize the RandomRateLimiter with appropriate parameters
        _randomRateLimiter = new RandomRateLimiter(1, 10);
    }

    [Test]
    public void AcquirePermit_ShouldReturnPermit()
    {
        // Act
        var permit = _randomRateLimiter.Acquire(1);

        // Assert
        Assert.IsNotNull(permit, "Permit should not be null");
    }

    [Test]
    public async Task AcquirePermitAsync_ShouldReturnPermit()
    {
        // Act
        var permit = await _randomRateLimiter.AcquireAsync(1);

        // Assert
        Assert.IsNotNull(permit, "Permit should not be null");
    }
}
