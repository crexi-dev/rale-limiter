namespace RateLimiter.Domain;

public class EmptyRateLimitDescriptor : RateLimitDescriptor
{
    public EmptyRateLimitDescriptor() : base(string.Empty, string.Empty)
    {
    }
}
