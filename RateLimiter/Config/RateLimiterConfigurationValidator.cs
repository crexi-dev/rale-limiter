using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Config
{
    public class RateLimiterConfigurationValidator : IRateLimiterConfigurationValidator
    {
        public (bool IsValid, List<string> Errors) Validate(RateLimiterConfiguration configuration)
        {
            var messages = new List<string>();

            foreach (var algorithm in configuration.Algorithms)
            {
                switch (algorithm.Type)
                {
                    case AlgorithmType.FixedWindow:
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.MaxRequests)}");
                        if (algorithm.Parameters.WindowDurationMS is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.WindowDurationMS)}");
                        break;
                    case AlgorithmType.LeakyBucket:
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.Capacity)}");
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.IntervalMS)}");
                        break;
                    case AlgorithmType.SlidingWindow:
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.MaxRequests)}");
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.WindowDurationMS)}");
                        break;
                    case AlgorithmType.TimespanElapsed:
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.MinIntervalMS)}");
                        break;
                    case AlgorithmType.TokenBucket:
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.MaxTokens)}");
                        if (algorithm.Parameters.MaxRequests is null)
                            messages.Add($"Algorithm {algorithm.Name} is of type {algorithm.Type}, but is missing {nameof(algorithm.Parameters.RefillRatePerSecond)}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var discriminator in configuration.Discriminators)
            {
                foreach (var algorithmName in discriminator.AlgorithmNames)
                {
                    var exists = configuration.Algorithms.Any(x => x.Name == algorithmName);
                    if (!exists)
                        messages.Add($"Discriminator {discriminator.Name} references an algorithm named {algorithmName}, but it does not exist in the configured algorithms");
                }
            }

            foreach (var rule in configuration.Rules)
            {
                foreach (var discriminatorName in rule.Discriminators)
                {
                    var exists = configuration.Discriminators.Any(x => x.Name == discriminatorName);
                    if (!exists)
                        messages.Add($"Rule {rule.Name} references a discriminator named {discriminatorName}, but it does not exist in the configured discriminators");
                }
            }

            return (messages.Count == 0,  messages);

        }
    }
}
