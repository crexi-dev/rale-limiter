namespace RateLimiter.Enums;

public enum LimiterDiscriminator
{
    Custom,
    GeoLocation,
    IpAddress,
    IpSubNet,
    QueryString,
    RequestHeader
}