namespace RateLimiter.Rules
{
    public interface IRateLimitRule
    {
        bool IsAllowed(string clientId, string resource);
        void Cleanup();
    }
}
