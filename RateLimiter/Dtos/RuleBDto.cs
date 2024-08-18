using System;

namespace RateLimiter.Dtos;

public class RuleBDto
{
    public DateTime LastCallDateTime { get; set; }
    public int RequestCount { get; set; }
}
