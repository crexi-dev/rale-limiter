namespace RateLimiter;

public class RateLimiterConfig
{
    public const string SectionName = "RateLimiterConfig";

    public int CacheExpirationInMinutes { get; set; }
}