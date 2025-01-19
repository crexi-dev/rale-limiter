namespace RateLimiter.Rules
{
    public interface IRateLimiterRule
    {
        bool IsAllowed(string token, string uri);
    }
}
