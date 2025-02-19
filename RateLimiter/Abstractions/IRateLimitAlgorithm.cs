using RateLimiter.Enums;

namespace RateLimiter.Abstractions;

public interface IRateLimitAlgorithm
{
    string Name { get; init; }

    bool IsAllowed(string discriminator);

    AlgorithmType AlgorithmType { get; init; }
}