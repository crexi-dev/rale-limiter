using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace RateLimiter;

/// <summary>
/// A rate limiter that applies a predicate to determine whether to limit the requests.
/// </summary>
/// <typeparam name="T">The type of resource being limited.</typeparam>
public class PredicatedRateLimiter<T> : PartitionedRateLimiter<T>
{
    private readonly Func<T, bool> _limitPredicate;
    private readonly PartitionedRateLimiter<T> _innerLimiter;
    private readonly bool _disposeInnerLimiter;

    private int _disposed;
    private static readonly RateLimitLease SuccessfulLease = new PredicateLease();

    /// <summary>
    /// Constructs the <see cref="PredicatedRateLimiter{T}"/>
    /// </summary>
    /// <param name="limitPredicate">The predicate to determine whether to limit the requests.</param>
    /// <param name="innerLimiter">The inner limiter to use for the actual limiting.</param>,
    /// <param name="disposeInnerLimiter">Whether to dispose the inner limiter when this limiter is disposed.</param>
    public PredicatedRateLimiter(Func<T, bool> limitPredicate, PartitionedRateLimiter<T> innerLimiter,
        bool disposeInnerLimiter = true)
    {
        _limitPredicate = limitPredicate;
        _innerLimiter = innerLimiter;
        _disposeInnerLimiter = disposeInnerLimiter;
    }

    /// <inheritdoc />
    public override RateLimiterStatistics? GetStatistics(T resource)
    {
        ThrowIfDispose();
        return _innerLimiter.GetStatistics(resource);
    }

    /// <inheritdoc />
    protected override RateLimitLease AttemptAcquireCore(T resource, int permitCount)
    {
        ThrowIfDispose();
        if (!_limitPredicate(resource))
        {
            return SuccessfulLease;
        }
        return _innerLimiter.AttemptAcquire(resource, permitCount);
    }

    /// <inheritdoc />
    protected override ValueTask<RateLimitLease> AcquireAsyncCore(T resource, int permitCount, CancellationToken cancellationToken)
    {
        ThrowIfDispose();
        if (!_limitPredicate(resource))
        {
            return ValueTask.FromResult(SuccessfulLease);
        }
        return _innerLimiter.AcquireAsync(resource, permitCount, cancellationToken);
    }
    
    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        if (_disposeInnerLimiter)
        {
            _innerLimiter.Dispose();
        }
    }
    
    private void ThrowIfDispose()
    {
        if (_disposed != 1) return;
        throw new ObjectDisposedException(nameof(PartitionedRateLimiter<T>));
    }
    
    private sealed class PredicateLease : RateLimitLease
    {
        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = default;
            return false;
        }

        public override bool IsAcquired { get; } = true;
        public override IEnumerable<string> MetadataNames { get; } = Array.Empty<string>();
    }
}