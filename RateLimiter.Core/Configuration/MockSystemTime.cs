using System;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Configuration;

/// <summary>
/// Only for test 
/// </summary>
/// <param name="startTime">Test start time</param>
public class MockSystemTime(DateTime startTime) : ISystemTime
{
    public DateTime CurrentTime { get; set; } = startTime;

    public DateTime GetCurrentUtcTime() => CurrentTime;
}