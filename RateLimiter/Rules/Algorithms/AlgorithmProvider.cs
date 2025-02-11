using Microsoft.Extensions.Options;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;

using System;

namespace RateLimiter.Rules.Algorithms
{
    public class AlgorithmProvider : IProvideRateLimitAlgorithms
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOptions<RateLimiterConfiguration> _options;

        public AlgorithmProvider(
            IDateTimeProvider dateTimeProvider,
            IOptions<RateLimiterConfiguration> options)
        {
            _dateTimeProvider = dateTimeProvider;
            _options = options;
        }

        public IAmARateLimitAlgorithm GetAlgorithm(
            RateLimitingAlgorithm algo,
            int? maxRequests,
            TimeSpan? timespanMilliseconds)
        {
            return algo switch
            {
                RateLimitingAlgorithm.Default or RateLimitingAlgorithm.FixedWindow => new FixedWindow(_dateTimeProvider,
                    new FixedWindowConfiguration()
                    {
                        MaxRequests = maxRequests ?? _options.Value.DefaultMaxRequests,
                        WindowDuration = timespanMilliseconds ?? TimeSpan.FromMilliseconds(_options.Value.DefaultTimespanMilliseconds)
                    }),
                RateLimitingAlgorithm.TokenBucket => new TokenBucket(_dateTimeProvider,
                    new TokenBucketConfiguration()
                    {
                        // TODO: Move to config
                        MaxTokens = 10,
                        RefillRatePerSecond = 10
                    }),
                RateLimitingAlgorithm.LeakyBucket => new LeakyBucket(_dateTimeProvider,
                    new LeakyBucketConfiguration()
                    {
                        Capacity = maxRequests ?? _options.Value.DefaultMaxRequests,
                        Interval = timespanMilliseconds ?? TimeSpan.FromMilliseconds(_options.Value.DefaultTimespanMilliseconds)
                    }),
                RateLimitingAlgorithm.SlidingWindow => new SlidingWindow(_dateTimeProvider,
                    new SlidingWindowConfiguration()
                    {
                        MaxRequests = maxRequests ?? _options.Value.DefaultMaxRequests,
                        WindowDuration = timespanMilliseconds ?? TimeSpan.FromMilliseconds(_options.Value.DefaultTimespanMilliseconds)
                    }),
                RateLimitingAlgorithm.TimespanElapsed => new TimespanElapsed(_dateTimeProvider,
                    new TimespanElapsedConfiguration()
                    {
                        MinInterval = timespanMilliseconds ?? TimeSpan.FromMilliseconds(_options.Value.DefaultTimespanMilliseconds)
                    }),
                _ => throw new ArgumentOutOfRangeException(nameof(algo), algo, null)
            };
        }
    }
}
