namespace RateLimiter.Core.Configuration.Contracts;

public interface IRateLimitRule
{
    bool IsAllowed(string clientToken, string resourceKey);
    void RecordRequest(string clientToken, string resourceKey);
}