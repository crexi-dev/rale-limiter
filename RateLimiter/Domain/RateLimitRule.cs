using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Domain;

public class RateLimitRule
{
    public string Domain { get; }
    public IEnumerable<RateLimitDescriptor> Descriptors { get; }

    public RateLimit RateLimit { get; }

    public RateLimitRule(
        string domain,
        IList<RateLimitDescriptor> descriptors,
        RateLimit rateLimit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(rateLimit);

        if (!descriptors.Any())
        {
            throw new ArgumentException($"No descriptors have been provided for argument {nameof(descriptors)}.");
        }

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
        if (!string.Equals(Domain, other.Domain, StringComparisonDefaults.DefaultStringComparison))
        {
            return false;
        }

        var thisDescriptors = Descriptors.OrderBy(d => d.Key, StringComparisonDefaults.DefaultStringComparer);

        var otherDescriptors = other.Descriptors.OrderBy(d => d.Key, StringComparisonDefaults.DefaultStringComparer);


        return thisDescriptors.SequenceEqual(otherDescriptors);
    }


    public override int GetHashCode()
    {
        var hash = new HashCode();
        
        hash.Add(Domain, StringComparisonDefaults.DefaultStringComparer);

        foreach (var descriptor in Descriptors.OrderBy(d => d.Key, StringComparisonDefaults.DefaultStringComparer))
        {
            hash.Add(descriptor);
        }

        return hash.ToHashCode();
    }
}
