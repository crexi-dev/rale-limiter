using System;

namespace RateLimiter;

public class RateLimitDescriptor
{
    public string Key { get; }
    public string Value { get; }

    public RateLimitDescriptor(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is RateLimitDescriptor other && Equals(other);
    }

    public bool Equals(RateLimitDescriptor other)
    {
        var stringComparison = StringComparison.InvariantCultureIgnoreCase;

        return string.Equals(Key, other.Key, stringComparison) &&
               string.Equals(Value, other.Value, stringComparison);
    }
            

    public override int GetHashCode()
    {
        var hash = new HashCode();
        var stringComparer = StringComparer.InvariantCultureIgnoreCase;

        hash.Add(Key, stringComparer);
        hash.Add(Value, stringComparer);

        return hash.ToHashCode();
    }
}