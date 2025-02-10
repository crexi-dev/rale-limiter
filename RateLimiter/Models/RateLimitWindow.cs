using System;

namespace RateLimiter.Models;

/// <summary>
/// Request start time and number of requests.
/// </summary>
public class RateLimitWindow
{
    /// <summary>
    /// Time at which first request was made. 
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of window (start time + timeSpan).
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Number of requests made.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Initializes an instance of <see cref="RateLimitWindow"/>.
    /// </summary>
    /// <param name="startTime">Start time of the window.</param>
    /// <param name="requestCount">Number of requests.</param>
    public RateLimitWindow(DateTime startTime, DateTime endTime, int requestCount) 
    {
        StartTime = startTime;
        EndTime = endTime;
        RequestCount = requestCount;
    }
}