using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules.Algorithms
{
    public class TimespanElapsed : IRateLimitAlgorithm
    {
        private readonly TimeSpan _minInterval;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ConcurrentDictionary<string, DateTime> _lastCallTimes = new();

        public TimespanElapsed(
            IDateTimeProvider dateTimeProvider,
            TimespanElapsedConfiguration configuration)
        {
            _dateTimeProvider = dateTimeProvider;
            _minInterval = configuration.MinInterval;
        }

        public string Name { get; init; } = nameof(TimespanElapsed);

        public bool IsAllowed(string discriminator)
        {
            var now = _dateTimeProvider.UtcNow();
            var lastCall = _lastCallTimes.GetOrAdd(discriminator, DateTime.MinValue);

            if ((now - lastCall) < _minInterval)
            {
                return false; // Too soon since last call
            }

            _lastCallTimes[discriminator] = now; // Update last call time
            return true;
        }

        public AlgorithmType AlgorithmType { get; init; } = AlgorithmType.TimespanElapsed;
    }
}
