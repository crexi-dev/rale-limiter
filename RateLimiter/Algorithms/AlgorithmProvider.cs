using Microsoft.Extensions.Options;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;
using System;
using System.Collections.Concurrent;

namespace RateLimiter.Algorithms
{
    public class AlgorithmProvider(IDateTimeProvider dateTimeProvider) : IRateLimitAlgorithmProvider
    {
        public ConcurrentDictionary<string, IRateLimitAlgorithm>
            GenerateAlgorithmsFromRules(RateLimiterConfiguration configuration)
        {
            var algorithms = new ConcurrentDictionary<string, IRateLimitAlgorithm>();

            foreach (var algo in configuration.Algorithms)
            {
                switch (algo.Type)
                {
                    case AlgorithmType.FixedWindow:
                        if (!algorithms.TryGetValue(algo.Name, out _))
                        {
                            algorithms.TryAdd(algo.Name, new FixedWindow(dateTimeProvider,
                                new FixedWindowConfiguration()
                                {
                                    MaxRequests = algo.Parameters.MaxRequests!.Value,
                                    WindowDuration = TimeSpan.FromMilliseconds(algo.Parameters.WindowDurationMS!.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.LeakyBucket:
                        if (!algorithms.TryGetValue(algo.Name, out _))
                        {
                            algorithms.TryAdd(algo.Name, new LeakyBucket(dateTimeProvider,
                                new LeakyBucketConfiguration()
                                {
                                    Capacity = algo.Parameters.Capacity!.Value,
                                    Interval = TimeSpan.FromMilliseconds(algo.Parameters.IntervalMS!.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.SlidingWindow:
                        if (!algorithms.TryGetValue(algo.Name, out _))
                        {
                            algorithms.TryAdd(algo.Name, new SlidingWindow(dateTimeProvider,
                                new SlidingWindowConfiguration()
                                {
                                    MaxRequests = algo.Parameters.MaxRequests!.Value,
                                    WindowDuration = TimeSpan.FromMilliseconds(algo.Parameters.WindowDurationMS!.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.TimespanElapsed:
                        if (!algorithms.TryGetValue(algo.Name, out _))
                        {
                            algorithms.TryAdd(algo.Name, new TimespanElapsed(dateTimeProvider,
                                new TimespanElapsedConfiguration()
                                {
                                    MinInterval = TimeSpan.FromMilliseconds(algo.Parameters.MinIntervalMS!.Value)
                                }));
                        }
                        break;
                    case AlgorithmType.TokenBucket:
                        if (!algorithms.TryGetValue(algo.Name, out _))
                        {
                            algorithms.TryAdd(algo.Name, new TokenBucket(dateTimeProvider,
                                new TokenBucketConfiguration()
                                {
                                    RefillRatePerSecond = algo.Parameters.RefillRatePerSecond!.Value,
                                    MaxTokens = algo.Parameters.MaxTokens!.Value
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
