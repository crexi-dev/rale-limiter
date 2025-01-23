namespace Crexi.RateLimiter.Models;

/// <summary>
/// PolicyType enum denotes the different algorithms available, similar naming as .NET RateLimiting
/// </summary>
public enum PolicyType
{
    SlidingWindow = 1,
    ConcurrentRequests = 2
}