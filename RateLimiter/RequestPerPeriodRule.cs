using System;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter;

public class RequestPerPeriodRule : IRateRule
{
    private readonly ITokenInspector _tokenInspector;
    private readonly IClientDb _clientDb;
    private readonly int _maxRequest;
    private readonly TimeSpan _period;

    public RequestPerPeriodRule(ITokenInspector tokenInspector, IClientDb clientDb, int maxRequest, TimeSpan period)
    {
        _tokenInspector = tokenInspector;
        _clientDb = clientDb;
        _maxRequest = maxRequest;
        _period = period != default ? 
            period : throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be set");
    }
    
    public async Task<CheckStatus> Check(string token, ApiEndpoint endpoint)
    {
        var tokenInfo = await _tokenInspector.GetInfo(token);
        var requestCount = await _clientDb.GetRequestCount(tokenInfo.Client, _period);
        
        if (requestCount >= _maxRequest)
            return new CheckStatus(false, "Request count exceeded for this period.");

        return new CheckStatus(true);
    }
}