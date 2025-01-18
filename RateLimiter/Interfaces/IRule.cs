namespace RateLimiter.Interfaces
{
    public interface IRule
    {
        bool IsAllowed(string clientId, string resource);
    }
}
