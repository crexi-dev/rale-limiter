namespace RateLimiter.Interfaces;
/// <summary>
/// Each Rule must return an implementation of the following interface
/// </summary>
public interface IRateLimiterResult
{
    bool Success { get; internal set; }
}
