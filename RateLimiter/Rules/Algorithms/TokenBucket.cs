using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules.Algorithms;

public class TokenBucket : IAmARateLimitAlgorithm
{
    private readonly double _maxTokens;
    private readonly double _refillRatePerSecond; // Tokens added per second
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ConcurrentDictionary<string, BucketState> _buckets;

    public string Name { get; init; } = nameof(TokenBucket);

    public bool IsAllowed(string discriminator)
    {
        var bucket = _buckets.GetOrAdd(discriminator, _ =>
            new BucketState { Tokens = _maxTokens, LastRefillTime = _dateTimeProvider.UtcNow() });

        lock (bucket.Lock)
        {
            var now = _dateTimeProvider.UtcNow();
            var timeElapsed = now - bucket.LastRefillTime;

            // Refill tokens based on elapsed time
            double tokensToAdd = timeElapsed.TotalSeconds * _refillRatePerSecond;
            bucket.Tokens = Math.Min(bucket.Tokens + tokensToAdd, _maxTokens);
            bucket.LastRefillTime = now;

            if (bucket.Tokens >= 1.0)
            {
                bucket.Tokens -= 1.0;
                return true;
            }

            return false;
        }
    }

    public RateLimitingAlgorithm Algorithm { get; init; } = RateLimitingAlgorithm.TokenBucket;

    private class BucketState
    {
        public double Tokens { get; set; }
        public DateTime LastRefillTime { get; set; }
        public object Lock { get; } = new object();
    }
}