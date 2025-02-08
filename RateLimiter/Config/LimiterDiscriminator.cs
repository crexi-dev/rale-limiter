namespace RateLimiter.Config;

public enum LimiterDiscriminator
{
    ApiKey,
    Custom,
    GeoLocation,
    IpAddress,
    IpSubNet,
    RequestHeader
}