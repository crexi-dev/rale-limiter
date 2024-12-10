using System;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter;

public class TimeElapseRule : IRateRule
{
    private readonly ITokenInspector _tokenInspector;
    private readonly IClientDb _clientDb;
    private readonly TimeSpan _period;
    
    public TimeElapseRule(ITokenInspector tokenInspector, IClientDb clientDb, TimeSpan period)
    {
        _tokenInspector = tokenInspector;
        _clientDb = clientDb;
        _period = period;
        Clock = new Clock();
    }

    public IClock Clock { get; set; }

    public async Task<CheckStatus> Check(string token, ApiEndpoint endpoint)
    {
        // Get the time that passed since the last call
        var tokenInfo = await _tokenInspector.GetInfo(token);
        var lastRequest = await _clientDb.GetLastRequestTime(tokenInfo.Client);
        var timeElapsed = (await Clock.Now()).Subtract(lastRequest);

        // Verify that a certain amount of time has passed since the last call
        if (timeElapsed < _period)
            return new CheckStatus(false, $"RequestPerTimespan rule failed. Must wait {_period.Seconds} secs before the next request.");

        return new CheckStatus(true);
    }
}