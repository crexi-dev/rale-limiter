using System;

namespace RateLimiter.Config;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RateLimitedResource : Attribute
{
    public string RuleName { get; set; }
}