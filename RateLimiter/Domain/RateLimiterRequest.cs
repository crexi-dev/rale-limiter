using System;

namespace RateLimiter.Domain;

public class RateLimiterRequest
{
    public string Domain { get; }
    public RateLimitDescriptor Descriptor { get; }

    public RateLimiterRequest(string domain, RateLimitDescriptor descriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentNullException.ThrowIfNull(descriptor);

        Domain = domain;
        Descriptor = descriptor;
    }
}
