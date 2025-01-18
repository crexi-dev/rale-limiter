namespace Crexi.RateLimiter.Models;

/// <summary>
/// Result class for an individual policy evaluation, this POCO is primarily used internally in the library
/// </summary>
public class RateLimitPolicyResult
{
    public required bool HasPassedPolicy { get; init; }
    public required string PolicyName { get; init; }
}