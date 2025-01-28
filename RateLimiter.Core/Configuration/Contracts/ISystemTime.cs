using System;

namespace RateLimiter.Core.Configuration.Contracts;

public interface ISystemTime
{
    DateTime GetCurrentUtcTime();
}