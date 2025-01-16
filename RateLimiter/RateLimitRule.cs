using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

public class RateLimitRule
{
    public string Domain { get; }
    public IEnumerable<RateLimitDescriptor> Descriptors { get; }

    public RateLimit RateLimit { get; }

    public RateLimitRule(
        string domain,
        IEnumerable<RateLimitDescriptor> descriptors,
        RateLimit rateLimit)
    {
        Domain = domain;
        Descriptors = descriptors;
        RateLimit = rateLimit;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is RateLimitRule other && Equals(other);
    }

    public bool Equals(RateLimitRule other)
    {
        var stringComparison = StringComparison.InvariantCultureIgnoreCase;

        if (!string.Equals(Domain, other.Domain, stringComparison))
        {
            return false;
        }

        var stringComparer = StringComparer.InvariantCultureIgnoreCase;

        var thisDescriptors = Descriptors.OrderBy(d => d.Key, stringComparer);

        var otherDescriptors = other.Descriptors.OrderBy(d => d.Key, stringComparer);


        return thisDescriptors.SequenceEqual(otherDescriptors);
    }


    public override int GetHashCode()
    {
        var hash = new HashCode();
        var stringComparer = StringComparer.InvariantCultureIgnoreCase;

        hash.Add(Domain, stringComparer);

        foreach (var descriptor in Descriptors.OrderBy(d => d.Key, stringComparer))
        {
            hash.Add(descriptor);
        }

        return hash.ToHashCode();
    }
}
