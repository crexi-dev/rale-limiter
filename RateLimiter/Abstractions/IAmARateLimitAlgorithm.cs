using RateLimiter.Enums;

namespace RateLimiter.Abstractions;

public interface IAmARateLimitAlgorithm
{
    string Name { get; init; }

    bool IsAllowed(string discriminator);

    RateLimitingAlgorithm Algorithm { get; init; }
}