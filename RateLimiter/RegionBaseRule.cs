using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter;

public class RegionBaseRule : IRateRule
{
    private readonly ITokenInspector _tokenInspector;
    private readonly IRateRule _usRule;
    private readonly IRateRule _euRule;

    public RegionBaseRule(ITokenInspector tokenInspector, IRateRule usRule, IRateRule euRule)
    {
        _tokenInspector = tokenInspector;
        _usRule = usRule;
        _euRule = euRule;
    }
    
    public async Task<CheckStatus> Check(string token, ApiEndpoint endpoint)
    {
        var tokenInfo = await _tokenInspector.GetInfo(token);

        if (tokenInfo.Region == Region.US)
            return await _usRule.Check(token, endpoint);
        
        if (tokenInfo.Region == Region.EU)
            return await _euRule.Check(token, endpoint);
        
        throw new System.NotImplementedException($"Region {tokenInfo.Region} not supported");
    }
}