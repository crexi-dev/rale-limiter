using Microsoft.Extensions.Options;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules.Algorithms
{
    public class AlgorithmProvider : IRateLimitAlgorithmProvider
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


        public ConcurrentDictionary<string, IRateLimitAlgorithm>
            GenerateAlgorithmsFromRules(RateLimiterConfiguration configuration)
        {
            var algorithms = new ConcurrentDictionary<string, IRateLimitAlgorithm>();

            foreach (var algo in configuration.Algorithms)
            {
                switch (algo.Type)
                {
                    case AlgorithmType.FixedWindow:
                        if (!algorithms.TryGetValue(algo.Name, out var existingAlgo))
                        {
                            algorithms.TryAdd(algo.Name, new FixedWindow(_dateTimeProvider,
                                new FixedWindowConfiguration()
                                {
                                    MaxRequests = algo.Parameters.MaxRequests.Value,
                                    WindowDuration = TimeSpan.FromMilliseconds(algo.Parameters.WindowDurationMS.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.LeakyBucket:
                        if (!algorithms.TryGetValue(algo.Name, out var existingLeaky))
                        {
                            algorithms.TryAdd(algo.Name, new LeakyBucket(_dateTimeProvider,
                                new LeakyBucketConfiguration()
                                {
                                    Capacity = algo.Parameters.Capacity.Value,
                                    Interval = TimeSpan.FromMilliseconds(algo.Parameters.IntervalMS.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.SlidingWindow:
                        if (!algorithms.TryGetValue(algo.Name, out var existingSliding))
                        {
                            algorithms.TryAdd(algo.Name, new SlidingWindow(_dateTimeProvider,
                                new SlidingWindowConfiguration()
                                {
                                    MaxRequests = algo.Parameters.MaxRequests.Value,
                                    WindowDuration = TimeSpan.FromMilliseconds(algo.Parameters.WindowDurationMS.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.TimespanElapsed:
                        if (!algorithms.TryGetValue(algo.Name, out var existingTSElapsed))
                        {
                            algorithms.TryAdd(algo.Name, new TimespanElapsed(_dateTimeProvider,
                                new TimespanElapsedConfiguration()
                                {
                                    MinInterval = TimeSpan.FromMilliseconds(algo.Parameters.MinIntervalMS.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.TokenBucket:
                        if (!algorithms.TryGetValue(algo.Name, out var existingTokenBucket))
                        {
                            algorithms.TryAdd(algo.Name, new TokenBucket(_dateTimeProvider,
                                new TokenBucketConfiguration()
                                {
                                    RefillRatePerSecond = algo.Parameters.RefillRatePerSecond.Value,
                                    MaxTokens = algo.Parameters.MaxTokens.Value
                                }));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return algorithms;
        }
    }
}
