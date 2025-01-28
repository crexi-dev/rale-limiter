using System;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Configuration;
/// <summary>
/// Real time usage
/// </summary>
public class SystemSystemTime : ISystemTime
{
    public DateTime GetCurrentUtcTime() => DateTime.UtcNow;
}