using System;

namespace RateLimiter.Domain;

public class RateLimitRuleState
{
    public int RequestsMade { get; }
    public DateTime WindowStart { get; }

    public RateLimitRuleState(int requestsMade, DateTime windowStart)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(requestsMade, nameof(requestsMade));
        
        RequestsMade = requestsMade;
        WindowStart = windowStart;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is RateLimitRuleState other && Equals(other);
    }

    public bool Equals(RateLimitRuleState other) =>
        RequestsMade == other.RequestsMade && WindowStart == other.WindowStart;


    public override int GetHashCode() =>
        HashCode.Combine(RequestsMade.GetHashCode(), WindowStart.GetHashCode());
}
