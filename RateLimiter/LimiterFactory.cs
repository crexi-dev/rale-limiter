using RateLimiter.Base;
using RateLimiter.Config;
using System;
using System.Collections.Generic;

namespace RateLimiter;

/// <summary>
/// Factory class to create rate limiters based on the provided configuration.
/// A potential enhancement to the solution would be to internalize all limiters and 
/// enforce access through this factory class.
/// </summary>
public class LimiterFactory
{
    /// <summary>
    /// Creates a rate limiter based on a single limiter configuration.
    /// </summary>
    /// <param name="limiterConfig">The configuration for the rate limiter.</param>
    /// <returns>A rate limiter instance.</returns>
    /// <exception cref="ArgumentException">Thrown when no configuration is provided.</exception>
    /// <exception cref="NotImplementedException">Thrown when the limiter type is not implemented.</exception>
    public static BaseRateLimiter CreateRateLimiter(LimiterConfig limiterConfig)
    {
        if (limiterConfig == null)
        {
            throw new ArgumentException("No rate limiter configuration provided.");
        }

        return limiterConfig.LimiterType switch
        {
            LimiterType.TokenLimiter => new TokenLimiter(limiterConfig),
            LimiterType.FixedWindowLimiter => new FixedWindowLimiter(limiterConfig),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a rate limiter based on a list of limiter configurations.
    /// </summary>
    /// <param name="limiterConfigs">The list of configurations for the rate limiters.</param>
    /// <returns>A rate limiter instance.</returns>
    /// <exception cref="ArgumentException">Thrown when no configurations are provided or when a LinkedLimiter type is found in the configuration.</exception>
    /// <exception cref="NotImplementedException">Thrown when a limiter type is not implemented.</exception>
    public static BaseRateLimiter CreateRateLimiter(List<LimiterConfig> limiterConfigs)
    {
        if (limiterConfigs == null || limiterConfigs.Count == 0)
        {
            throw new ArgumentException("No rate limiter configurations provided.");
        }

        // Linked limiter is self-configuring, so we shouldn't have any LinkedLimiter types in the configuration.
        foreach (var limiterConfig in limiterConfigs)
        {
            if (limiterConfig.LimiterType == LimiterType.LinkedLimiter)
            {
                throw new ArgumentException("LinkedLimiter type is not allowed in the configuration.");
            }
        }

        if (limiterConfigs.Count == 1)
        {
            return CreateRateLimiter(limiterConfigs[0]);
        }

        var linkedLimierConfig = new LimiterConfig { LimiterType = LimiterType.LinkedLimiter };
        var rateLimiters = new List<BaseRateLimiter>();
        foreach (var rateLimiterConfiguration in limiterConfigs)
        {
            rateLimiters.Add(rateLimiterConfiguration.LimiterType switch
            {
                LimiterType.TokenLimiter => new TokenLimiter(rateLimiterConfiguration),
                LimiterType.FixedWindowLimiter => new FixedWindowLimiter(rateLimiterConfiguration),
                _ => throw new NotImplementedException(),
            });
        }
        return new LinkedLimiter(rateLimiters, linkedLimierConfig);
    }
}