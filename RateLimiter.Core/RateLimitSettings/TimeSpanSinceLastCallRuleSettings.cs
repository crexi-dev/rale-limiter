﻿using RateLimiter.Contracts;

namespace RateLimiter.RateLimitSettings;

public class TimeSpanSinceLastCallRuleSettings : ISetting
{
    public int MinimumIntervalInMinutes { get; init; }
}