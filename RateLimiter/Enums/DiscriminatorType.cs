namespace RateLimiter.Enums;

public enum DiscriminatorType
{
    Custom,
    GeoLocation,
    IpAddress,
    IpSubNet,
    QueryString,
    RequestHeader
}