namespace RateLimiter;

public class EmptyRateLimitDescriptor : RateLimitDescriptor
{
    public EmptyRateLimitDescriptor() : base(string.Empty, string.Empty)
    {
    }
}