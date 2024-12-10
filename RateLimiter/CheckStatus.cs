namespace RateLimiter;

public struct CheckStatus
{
    public bool WithInLimit { get; }
    public string Message { get; }

    public CheckStatus(bool withInLimit, string message = null)
    {
        WithInLimit = withInLimit;
        Message = message ?? string.Empty;
    }
}