using RateLimiter.CustomLimiters;
using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace RateLimiter;

/// <summary>
/// Provides functionality to create and manage resource-specific rate limiters.
/// </summary>
public class ResourceRateLimiter
{
    /// <summary>
    /// Enum representing different types of rate limiters.
    /// </summary>
    public enum RateLimiterType
    {
        ConcurrencyLimiter,
        FixedWindowRateLimiter,
        RandomRateLimiter,
        SlidingWindowRateLimiter,
        TokenBucketRateLimiter
    };

    /// <summary>
    /// Creates resource rate limiters based on the provided configurations.
    /// </summary>
    /// <param name="resourceLimitConfigs">List of resource limit configurations.</param>
    /// <returns>List of tuples containing resource names and their corresponding rate limiters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when resourceLimitConfigs is null.</exception>
    /// <exception cref="ArgumentException">Thrown when resource name is not specified or duplicate resource names are found.</exception>
    public List<(string Resource, PartitionedRateLimiter<string> RateLimiter)> CreateResourceRateLimiters(List<ResourceLimitConfig> resourceLimitConfigs)
    {
        if (resourceLimitConfigs == null)
            throw new ArgumentNullException(nameof(resourceLimitConfigs));

        var resourceLimiters = new List<(string ResourceName, PartitionedRateLimiter<string> RateLimiter)>();
        var resourceNames = new HashSet<string>();

        foreach (var resource in resourceLimitConfigs)
        {
            if (string.IsNullOrWhiteSpace(resource.Name))
                throw new ArgumentException("Resource name must be specified.");

            if (!resourceNames.Add(resource.Name))
                throw new ArgumentException("Duplicate resource names are not allowed.");

            var limiters = new List<PartitionedRateLimiter<string>>();

            foreach (var limiterConfig in resource.Limiters)
            {
                // Create a rate limiter based on the configuration
                var rateLimiter = CreateRateLimiter(resource.Name, limiterConfig);
                limiters.Add(rateLimiter);
            }

            // Create a chained rate limiter for the resource
            resourceLimiters.Add((resource.Name, PartitionedRateLimiter.CreateChained(limiters.ToArray())));
        }

        return resourceLimiters;
    }

    /// <summary>
    /// Creates a specific rate limiter based on the configuration.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="limiterConfig">Configuration for the rate limiter.</param>
    /// <returns>A partitioned rate limiter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unknown limiter type is specified.</exception>
    private static PartitionedRateLimiter<string> CreateRateLimiter(string resourceName, RateLimiterConfig limiterConfig)
    {
        return limiterConfig.LimiterType switch
        {
            RateLimiterType.ConcurrencyLimiter => CreateConcurrencyLimiter(resourceName, limiterConfig),
            RateLimiterType.FixedWindowRateLimiter => CreateFixedWindowRateLimiter(resourceName, limiterConfig),
            RateLimiterType.RandomRateLimiter => CreateRandomRateLimiter(resourceName, limiterConfig),
            RateLimiterType.SlidingWindowRateLimiter => CreateSlidingWindowRateLimiter(resourceName, limiterConfig),
            RateLimiterType.TokenBucketRateLimiter => CreateTokenBucketRateLimiter(resourceName, limiterConfig),
            _ => throw new InvalidOperationException($"Unknown limiter type: {limiterConfig.LimiterType}")
        };
    }

    /// <summary>
    /// Creates a concurrency limiter. See <see cref="ConcurrencyLimiter"/> for limiter details.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="config">Configuration for the concurrency limiter.</param>
    /// <returns>A partitioned concurrency limiter.</returns>
    private static PartitionedRateLimiter<string> CreateConcurrencyLimiter(string resourceName, RateLimiterConfig config) =>
        PartitionedRateLimiter.Create<string, string>(
            resource => new RateLimitPartition<string>(resourceName, key => new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = config.PermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = config.QueueLimit
            }))
        );

    /// <summary>
    /// Creates a fixed window rate limiter. See <see cref="FixedWindowRateLimiter"/> for limiter details.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="config">Configuration for the fixed window rate limiter.</param>
    /// <returns>A partitioned fixed window rate limiter.</returns>
    private static PartitionedRateLimiter<string> CreateFixedWindowRateLimiter(string resourceName, RateLimiterConfig config) =>
        PartitionedRateLimiter.Create<string, string>(
            resource => new RateLimitPartition<string>(resourceName, key => new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = config.PermitLimit,
                Window = TimeSpan.FromMinutes(config.WindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = config.QueueLimit
            }))
        );

    /// <summary>
    /// Creates a random rate limiter. See <see cref="RandomRateLimiter"/> for limiter details.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="config">Configuration for the random rate limiter.</param>
    /// <returns>A partitioned random rate limiter.</returns>
    private static PartitionedRateLimiter<string> CreateRandomRateLimiter(string resourceName, RateLimiterConfig config) =>
        PartitionedRateLimiter.Create<string, string>(
            resource => new RateLimitPartition<string>(resourceName, key => new RandomRateLimiter(config.MinPermits, config.PermitLimit))
        );

    /// <summary>
    /// Creates a sliding window rate limiter. See <see cref="SlidingWindowRateLimiter"/> for limiter details.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="config">Configuration for the sliding window rate limiter.</param>
    /// <returns>A partitioned sliding window rate limiter.</returns>
    private static PartitionedRateLimiter<string> CreateSlidingWindowRateLimiter(string resourceName, RateLimiterConfig config) =>
        PartitionedRateLimiter.Create<string, string>(
            resource => new RateLimitPartition<string>(resourceName, key => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = config.PermitLimit,
                Window = TimeSpan.FromMinutes(config.WindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = config.QueueLimit,
                SegmentsPerWindow = config.SlidingWindowSegments
            }))
        );

    /// <summary>
    /// Creates a token bucket rate limiter. See <see cref="TokenBucketRateLimiter"/> for limiter details.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="config">Configuration for the token bucket rate limiter.</param>
    /// <returns>A partitioned token bucket rate limiter.</returns>
    private static PartitionedRateLimiter<string> CreateTokenBucketRateLimiter(string resourceName, RateLimiterConfig config) =>
        PartitionedRateLimiter.Create<string, string>(
            resource => new RateLimitPartition<string>(resourceName, key => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = config.PermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = config.QueueLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(config.ReplenishmentPeriodSeconds),
                TokensPerPeriod = config.TokensPerPeriod,
                AutoReplenishment = true
            }))
        );
}
