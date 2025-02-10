using RateLimiter.Enums;

namespace RateLimiter.Abstractions;

public interface IDefineRateLimitRules
{
    LimiterType Type { get; }

    string Name { get; set; }

    LimiterDiscriminator Discriminator { get; set; }

    string? CustomDiscriminatorName { get; set; }

    string? DiscriminatorRequestHeaderKey { get; set; }

    string? DiscriminatorMatch { get; set; }

    RateLimitingAlgorithm Algorithm { get; set; }
}