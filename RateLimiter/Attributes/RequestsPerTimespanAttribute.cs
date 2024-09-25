using System;

namespace RateLimiter.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequestsPerTimespanAttribute : Attribute, IRateLimit
{
    public int Rate { get; }
    public int Seconds { get; }
    public string ErrorMessage { get; }

    public RequestsPerTimespanAttribute(int rate, int seconds)
    {
        Rate = rate;
        Seconds = seconds;
        ErrorMessage = "Allowed requests exceeded";
    }

    public RequestsPerTimespanAttribute(int rate, int seconds, string errorMessage)
    {
        Rate = rate;
        Seconds = seconds;
        ErrorMessage = errorMessage;
    }

}

public interface IRateLimit
{
    string ErrorMessage { get; }

}