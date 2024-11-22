using System;
using System.Collections.Generic;

public class LimiterConfig
{
    public List<ResourceLimiterConfig> LimiterConfig { get; set; }
}

public class ResourceLimiterConfig
{
    public List<string> Resources { get; set; }
    public List<Limiter> Limiters { get; set; }
}

public class Limiter
{
    public string LimiterType { get; set; }
    public int MinPermits { get; set; }
    public int PermitLimit { get; set; }
    public int QueueLimit { get; set; }
    public int WindowMinutes { get; set; }
    public int TokensPerPeriod { get; set; }
    public int ReplenishmentPeriodSeconds { get; set; }
    public int SlidingWindowSegments { get; set; }
}
