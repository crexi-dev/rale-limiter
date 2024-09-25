using System;

namespace RateLimiter.Contracts;

public class RequestDetails
{
    private readonly DateTime _requestTime;

    private readonly Guid _token;

    public string RequestId => HashGenerator.GenerateHash(_token.ToString(), Resource);

    public DateTime RequestTime => _requestTime.ToUniversalTime();

    public string Location { get; }

    public string Resource { get; }

    public RequestDetails()
    {
        
    }

    public RequestDetails(string token, string location, string resource, DateTime? requestTime)
    {
        _requestTime = requestTime ?? DateTime.Now;
        _token = Guid.Parse(token);
        Location = location;
        Resource = resource;
    }
}