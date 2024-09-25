using System;

namespace RateLimiter.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CooldownPeriodAttribute : Attribute, IRateLimit
{
    public TimeSpan TimeSpan { get; }
    public string ErrorMessage { get; }

    public CooldownPeriodAttribute(int milliseconds)
    {
        TimeSpan = TimeSpan.FromMilliseconds(milliseconds);
        ErrorMessage = "Cooling period is in process. Please try again later.";
    }

    public CooldownPeriodAttribute(TimeSpan timeSpan, string errorMessage)
    {
        TimeSpan = timeSpan;
        ErrorMessage = errorMessage;
    }

}