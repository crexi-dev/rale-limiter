using RateLimiter.Geo;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Attributes;
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class GeoBasedRateLimitAttribute(string country, int maxRequests, double windowSeconds) : RateLimitAttribute
{
    public string Country { get; } = country;
    public int MaxRequests { get; } = maxRequests;
    public double UsWindowSeconds { get; } = windowSeconds;    

    public override IRateLimitRule CreateRule(IServiceProvider serviceProvider)
    {
        var geoService = (IGeoService)serviceProvider.GetService(typeof(IGeoService));
        IRateLimitRule rule;

        if (MaxRequests > 0)
        {
            rule = new FixedWindowRule(MaxRequests, TimeSpan.FromSeconds(UsWindowSeconds));
        }
        else
        {
            rule =  new FixedDelayRule(TimeSpan.FromSeconds(windowSeconds));
        }

        return new GeoBasedRule(country, rule, geoService);
    }
}
