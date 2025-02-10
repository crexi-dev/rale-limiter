using RateLimiter.Enums;

using System;

namespace RateLimiter.Rules;

public record FixedWindowConfiguration
{
    public string Name { get; set; }

    public int MaxRequests { get; init; }

    public TimeSpan WindowDuration { get; init; }

    public LimiterDiscriminator Discriminator { get; init; }

    public string? CustomDiscriminatorType { get; init; }
}