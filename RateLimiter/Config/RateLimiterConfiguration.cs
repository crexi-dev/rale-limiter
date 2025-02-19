using RateLimiter.Enums;

using System.Collections.Generic;

namespace RateLimiter.Config;

public class RateLimiterConfiguration
{
    public List<AlgorithmConfiguration> Algorithms { get; set; } = new();

    public List<DiscriminatorConfiguration> Discriminators { get; set; } = new();

    public List<RuleConfiguration> Rules { get; set; } = new();

    public class AlgorithmConfiguration
    {
        public string Name { get; set; }

        public AlgorithmType Type { get; set; }

        public AlgorithmConfigurationParameters Parameters { get; set; }

        public class AlgorithmConfigurationParameters
        {
            public int? MinIntervalMS { get; set; }

            public int? MaxRequests { get; set; }

            public int? WindowDurationMS { get; set; }

            public int? Capacity { get; set; }

            public int? IntervalMS { get; set; }

            public int? MaxTokens { get; set; }

            public int? RefillRatePerSecond { get; set; }
        }
    }

    public class RuleConfiguration
    {
        public string Name { get; set; }

        public List<string> Discriminators { get; set; } = new();
    }

    public class DiscriminatorConfiguration
    {
        public string Name { get; set; }

        public DiscriminatorType Type { get; set; }

        public string? CustomDiscriminatorType { get; set; }

        public string? DiscriminatorKey { get; set; }

        public string? DiscriminatorMatch { get; set; }

        public List<string> AlgorithmNames { get; set; } = new();
    }
}