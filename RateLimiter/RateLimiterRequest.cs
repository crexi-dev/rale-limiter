namespace RateLimiter;

public class RateLimiterRequest
{
    public string Domain { get; }
    public RateLimitDescriptor Descriptor { get; }

    public RateLimiterRequest(string domain, RateLimitDescriptor descriptor)
    {
        Domain = domain;
        Descriptor = descriptor;
    }
}
