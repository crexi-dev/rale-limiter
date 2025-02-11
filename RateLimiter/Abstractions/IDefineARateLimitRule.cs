using RateLimiter.Enums;

namespace RateLimiter.Abstractions;

public interface IDefineARateLimitRule
{
    LimiterType Type { get; }

    string Name { get; set; }

    LimiterDiscriminator Discriminator { get; set; }

    string? CustomDiscriminatorName { get; set; }

    string? DiscriminatorKey { get; set; }

    string? DiscriminatorMatch { get; set; }

    RateLimitingAlgorithm Algorithm { get; set; }
}