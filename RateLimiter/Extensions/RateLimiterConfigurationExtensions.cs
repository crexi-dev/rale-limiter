using RateLimiter.Configuration;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Extensions
{
    public static class RateLimiterConfigurationExtensions
    {
        public static IRateLimiterConfiguration AddMinimumTimeIntervalRule(this IRateLimiterConfiguration config, string uri, int minTimeIntervalInSeconds)
        {
            ArgumentNullException.ThrowIfNull(nameof(config));

            config.AddRule(uri, new MinimumTimeIntervalRule(config.DataStore, config.DateTimeProvider, minTimeIntervalInSeconds));
            return config;
        }

        public static IRateLimiterConfiguration AddRequestPerTimeSpanRule(this IRateLimiterConfiguration config, string uri, int maxRequests, int timespanInSeconds)
        {
            ArgumentNullException.ThrowIfNull(nameof(config));

            config.AddRule(uri, new RequestPerTimeSpanRule(config.DataStore, maxRequests, timespanInSeconds));
            return config;
        }
    }
}
