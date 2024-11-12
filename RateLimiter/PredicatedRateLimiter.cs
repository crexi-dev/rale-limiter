using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace RateLimiter;

public static class PredicatedRateLimiter
{
    public static PredicatedRateLimiter<TResource> Create<TResource, TKey>(
        Func<TResource, bool> limitPredicate,
        Func<TResource, RateLimitPartition<TKey>> partitionSelector,
        IEqualityComparer<TKey>? equalityComparer = null)
        where TKey : notnull
    {
        return new PredicatedRateLimiter<TResource>(limitPredicate,
            PartitionedRateLimiter.Create(partitionSelector, equalityComparer));
    }
    
    public static PredicatedRateLimiter<TResource> CreatePredicatedConcurrencyLimiter<TResource, TKey>(
        Func<TResource, bool> limitPredicate,
        TKey partitionKey,
        Func<TKey, ConcurrencyLimiterOptions> optionsSelector,
        IEqualityComparer<TKey>? equalityComparer = null) where TKey : notnull
    {
        return Create(limitPredicate, _ => RateLimitPartition.GetConcurrencyLimiter(partitionKey, optionsSelector),
            equalityComparer);
    }
    
    public static PredicatedRateLimiter<TResource> CreatePredicatedTokenBucketLimiter<TResource, TKey>(
        Func<TResource, bool> limitPredicate,
        TKey partitionKey,
        Func<TKey, TokenBucketRateLimiterOptions> optionsSelector,
        IEqualityComparer<TKey>? equalityComparer = null) where TKey : notnull
    {
        return Create(limitPredicate, _ => RateLimitPartition.GetTokenBucketLimiter(partitionKey, optionsSelector),
            equalityComparer);
    }
    
    public static PredicatedRateLimiter<TResource> CreatePredicatedSlidingWindowLimiter<TResource, TKey>(
        Func<TResource, bool> limitPredicate,
        TKey partitionKey,
        Func<TKey, SlidingWindowRateLimiterOptions> optionsSelector,
        IEqualityComparer<TKey>? equalityComparer = null) where TKey : notnull
    {
        return Create(limitPredicate, _ => RateLimitPartition.GetSlidingWindowLimiter(partitionKey, optionsSelector),
            equalityComparer);
    }

    public static PredicatedRateLimiter<TResource> CreatePredicatedFixedWindowLimiter<TResource, TKey>(
        Func<TResource, bool> limitPredicate,
        TKey partitionKey,
        Func<TKey, FixedWindowRateLimiterOptions> optionsSelector,
        IEqualityComparer<TKey>? equalityComparer = null) where TKey : notnull
    {
        return Create(limitPredicate, _ => RateLimitPartition.GetFixedWindowLimiter(partitionKey, optionsSelector),
            equalityComparer);
    }

    public static PredicatedRateLimiter<TResource> CreatePredicatedTimeSinceLimiter<TResource, TKey>(
        Func<TResource, bool> limitPredicate,
        TKey partitionKey,
        Func<TKey, TimeSinceLimiterOptions> optionsSelector,
        IEqualityComparer<TKey>? equalityComparer = null) where TKey : notnull
    {
        return Create(limitPredicate, _ => RateLimitPartition.Get(partitionKey, key =>
        {
            var options = optionsSelector(key);
            return new TimeSinceLimiter(options);
        }), equalityComparer);
    }
}