namespace RateLimiter.Common.Exceptions;

public class RateLimitExceededException(string message) : Exception(message)
{
    public RateLimitExceededException(string resource, int requestCount, int maxRequests, TimeSpan timeSpan) :
    this($"Rate limit exceeded for requested resource ({resource}) : Only {maxRequests} requests allowed per {timeSpan.TotalSeconds} seconds.")
    {
        Resource = resource;
        RequestCount = requestCount;
        MaxRequests = maxRequests;
        TimeSpan = timeSpan;
    }

    public RateLimitExceededException(string resource, TimeSpan timeSinceLastRequest, int maxRequests, TimeSpan timeSpan) :
    this($"Rate limit exceeded for requested resource ({resource}) : Minimum {timeSinceLastRequest.TotalSeconds} seconds required between requests.")
    {
        TimeSinceLastRequest = timeSinceLastRequest;
        MaxRequests = maxRequests;
        TimeSpan = timeSpan;
    }

    public string Resource { get; } = string.Empty;
    public int RequestCount { get; }
    public int MaxRequests { get; }
    public TimeSpan TimeSpan { get; }
    public TimeSpan TimeSinceLastRequest { get; }
}
