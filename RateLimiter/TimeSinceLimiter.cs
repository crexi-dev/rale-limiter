using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;


namespace RateLimiter;

/// <summary>
/// The TimeSinceLimiter class is a rate limiter that restricts actions to occur after a defined period.
/// </summary>
public class TimeSinceLimiter : System.Threading.RateLimiting.RateLimiter
{
    private long? _lastCallTick;
    private long? _idleSince;
    
    private readonly TimeSinceLimiterOptions _options;
    
    private object Lock => new();

    private long _failedLeaseCount;
    private long _successfulLeaseCount;
    
    /// <inheritdoc />
    public override TimeSpan? IdleDuration => _idleSince is null ? null : Stopwatch.GetElapsedTime(_idleSince.Value);
    
    private static readonly RateLimitLease SuccessfulLease = new TimeSinceLease(true, null);

    /// <summary>
    /// Initializes the <see cref="TimeSinceLimiter"/>
    /// </summary>
    /// <param name="options">Options to specify the behavior of the <see cref="TimeSinceLimiter"/></param>
    public TimeSinceLimiter(TimeSinceLimiterOptions options)
    {
        if (options.AllowedPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                $"{nameof(options.AllowedPeriod)} must be set to a value greater than TimeSpan.Zero.");
        }
        _options = new TimeSinceLimiterOptions
        {
            AllowedPeriod = options.AllowedPeriod
        };
        _idleSince = Stopwatch.GetTimestamp();
    }
    
    /// <inheritdoc />
    public override RateLimiterStatistics? GetStatistics()
    {
        return new RateLimiterStatistics
        {
            CurrentAvailablePermits = AllowedPeriodHasPassed() ? 1 : 0,
            CurrentQueuedCount = 0,
            TotalFailedLeases = Interlocked.Read(ref _failedLeaseCount),
            TotalSuccessfulLeases = Interlocked.Read(ref _successfulLeaseCount)
        };
    }

    /// <inheritdoc />
    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        if (permitCount > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(permitCount), permitCount, "Only a single permit is supported for TimeSinceLimiters.");
        }

        lock (Lock)
        {
            if (AllowedPeriodHasPassed())
            {
                _lastCallTick = Stopwatch.GetTimestamp();
                Interlocked.Increment(ref _successfulLeaseCount);
                return SuccessfulLease;
            }
            Interlocked.Increment(ref _failedLeaseCount);
            var retryAfter = Stopwatch.GetElapsedTime(Stopwatch.GetTimestamp(), _lastCallTick!.Value + _options.AllowedPeriod.Ticks);
            return new TimeSinceLease(false, retryAfter);
        }
    }

    /// <inheritdoc />
    protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(AttemptAcquireCore(permitCount));
    }

    private bool AllowedPeriodHasPassed()
    {
        if (_lastCallTick is null) return true;
        
        return Stopwatch.GetElapsedTime(_lastCallTick.Value) >= _options.AllowedPeriod;
    }

    private sealed class TimeSinceLease(bool isAcquired, TimeSpan? retryAfter) : RateLimitLease
    {
        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (metadataName == MetadataName.RetryAfter.Name && retryAfter.HasValue)
            {
                metadata = retryAfter.Value;
                return true;
            }

            metadata = default;
            return false;
        }

        public override bool IsAcquired { get; } = isAcquired;
        public override IEnumerable<string> MetadataNames => [MetadataName.RetryAfter.Name];
    }
}