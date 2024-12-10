namespace RateLimiter.Domain;

public class RequestAccessStatus
{
    public bool AccessGranted { get; }

    public string Message { get; }

    public RequestAccessStatus(bool accessGranted, string message)
    {
        AccessGranted = accessGranted;
        Message = message;
    }
}