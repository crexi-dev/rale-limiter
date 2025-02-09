using RateLimiter.Config;

namespace RateLimiter.Abstractions;

public interface IRateLimitRule
{
    string Name { get; set; }

    bool IsAllowed(string discriminator);

    LimiterDiscriminator Discriminator { get; set; }
}