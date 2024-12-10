using System;

namespace RateLimiter;

public static class UsefulExtensions
{
    public static DateTime ToDateTime(this string dateTimeString)
    {
        return DateTime.Parse(dateTimeString);
    }

    public static TimeSpan Seconds(this int secs)
    {
        return new TimeSpan(0, 0, secs);
    }
}