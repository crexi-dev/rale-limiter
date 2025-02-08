using System;

namespace RateLimiter.Config;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RateLimited : Attribute
{
    public LimiterType LimiterType { get; set; }

    public string Config { get; set; }

    public LimiterDiscriminator Discriminator { get; set; }
}

public enum LimiterType
{
    RequestsPerTimespan,
    TimespanElapsed
}

public enum LimiterDiscriminator
{
    IpAddress,
    GeoLocation,
    IpSubNet
}