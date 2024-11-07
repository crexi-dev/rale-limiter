using System;
using System.Threading.RateLimiting;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.CustomLimiters;

/// <summary>
/// A rate limiter that randomly determines the number of available permits within a specified range.
/// </summary>
public class RandomRateLimiter : System.Threading.RateLimiting.RateLimiter
{
    private readonly Random _random = new();
    private readonly int _minPermits;
    private readonly int _maxPermits;
    private int _currentPermits;

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomRateLimiter"/> class.
    /// </summary>
    /// <param name="minPermits">The minimum number of permits.</param>
    /// <param name="maxPermits">The maximum number of permits.</param>
    /// <exception cref="ArgumentException">Thrown when minPermits is greater than maxPermits.</exception>
    public RandomRateLimiter(int minPermits, int maxPermits)
    {
        if (minPermits > maxPermits)
        {
            throw new ArgumentException("minPermits cannot be greater than maxPermits");
        }

        _minPermits = minPermits;
        _maxPermits = maxPermits;
        _currentPermits = _random.Next(_minPermits, _maxPermits + 1);
    }

    /// <summary>
    /// Gets the idle duration of the rate limiter.
    /// </summary>
    public override TimeSpan? IdleDuration => null;

    /// <summary>
    /// Gets the statistics of the rate limiter.
    /// </summary>
    /// <returns>The statistics of the rate limiter.</returns>
    public override RateLimiterStatistics? GetStatistics()
    {
        return new RateLimiterStatistics
        {
            CurrentAvailablePermits = _currentPermits,
        };
    }

    /// <summary>
    /// Attempts to acquire the specified number of permits.
    /// </summary>
    /// <param name="permitCount">The number of permits to acquire.</param>
    /// <returns>A <see cref="RateLimitLease"/> representing the result of the acquisition attempt.</returns>
    public RateLimitLease Acquire(int permitCount)
    {
        return AttemptAcquireCore(permitCount);
    }

    /// <summary>
    /// Attempts to acquire the specified number of permits.
    /// </summary>
    /// <param name="permitCount">The number of permits to acquire.</param>
    /// <returns>A <see cref="RateLimitLease"/> representing the result of the acquisition attempt.</returns>
    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        lock (_random)
        {
            // Check if there are enough permits and randomly decide if the acquisition is successful
            if (_currentPermits >= permitCount && _random.NextDouble() > 0.5)
            {
                _currentPermits -= permitCount;
                return new RandomRateLimiterLease(true);
            }
            return new RandomRateLimiterLease(false);
        }
    }

    /// <summary>
    /// Asynchronously attempts to acquire the specified number of permits.
    /// </summary>
    /// <param name="permitCount">The number of permits to acquire.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{RateLimitLease}"/> representing the result of the acquisition attempt.</returns>
    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        // Simulate a delay before attempting to acquire permits
        await Task.Delay(_random.Next(100, 500), cancellationToken);
        return AttemptAcquireCore(permitCount);
    }
}
