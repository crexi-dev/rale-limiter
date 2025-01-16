using System;

namespace RateLimiter.Domain;

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
        return string.Equals(Key, other.Key, StringComparisonDefaults.DefaultStringComparison) &&
               string.Equals(Value, other.Value, StringComparisonDefaults.DefaultStringComparison);
    }
            

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Key, StringComparisonDefaults.DefaultStringComparer);
        hash.Add(Value, StringComparisonDefaults.DefaultStringComparer);

        return hash.ToHashCode();
    }
}
