using System;
using RateLimiter.Domain;
using System.Threading.Tasks;

namespace RateLimiter;

public class RateLimiter
{
    private readonly IRateRule[] _rateRules;
    private readonly string _client;

    public RateLimiter(string client, params IRateRule[] rateRules)
    {
        _client = !string.IsNullOrWhiteSpace(client) ? 
            client : throw new ArgumentException("Value cannot be null or whitespace.", nameof(client));
        
        _rateRules = rateRules;
    }

    private static RequestAccessStatus AccessGranted() => new AccessGranted();

    private static RequestAccessStatus AccessDenied(string message) => new AccessDenied(message);
    
    public async Task<RequestAccessStatus> CanAccess(ApiEndpoint endpoint)
    {
        // Check all the rules
        foreach (var rateRule in _rateRules)
        {
            var status = await rateRule.Check(_client, endpoint);

            if (!status.WithInLimit)
                return AccessDenied(status.Message);
        }
        
        // If all the rules pass, return a passing status
        return AccessGranted();
    }

}