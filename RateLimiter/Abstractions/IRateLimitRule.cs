using RateLimiter.Config;

namespace RateLimiter.Abstractions;

public interface IRateLimitRule
{
    bool IsAllowed(string discriminator);

    LimiterDiscriminator Discriminator { get; set; }
}