namespace RateLimiter.Enums;

public enum LimiterDiscriminator
{
    ApiKey,
    Custom,
    GeoLocation,
    IpAddress,
    IpSubNet,
    RequestHeader
}