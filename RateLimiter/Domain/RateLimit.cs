using System;

namespace RateLimiter.Domain;

public class RateLimit
{
    public int MaxRequests { get; }
    public TimeSpan WindowDuration { get; }

    public RateLimit(int maxRequests, TimeSpan windowDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRequests, nameof(maxRequests));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowDuration, TimeSpan.Zero, nameof(windowDuration));
        
        MaxRequests = maxRequests;
        WindowDuration = windowDuration;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is RateLimit other && Equals(other);
    }

    public bool Equals(RateLimit other) =>
        MaxRequests == other.MaxRequests && WindowDuration == other.WindowDuration;

    public override int GetHashCode() => HashCode.Combine(MaxRequests, WindowDuration);
}
