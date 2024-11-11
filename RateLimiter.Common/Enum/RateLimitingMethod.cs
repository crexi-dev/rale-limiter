namespace RateLimiter.Common.Enum;

[Flags]
public enum RateLimitingMethod
{
    None = 0,
    RequestsPerTimespan = 1 << 0,
    TimeSpanSinceLastRequest = 1 << 1,
}
