namespace Services.Common.RateLimitRules;

public class RegionBasedRule : IRateLimitRule
{
    private readonly IRateLimitRule _usRule;
    private readonly IRateLimitRule _euRule;

    public RegionBasedRule(IRateLimitRule usRule, IRateLimitRule euRule)
    {
        _usRule = usRule;
        _euRule = euRule;
    }

    public bool IsRequestAllowed(Guid token)
    {
        var region = GetRegionFromToken(token); // Assume implementation exists
        return region == "US" ? _usRule.IsRequestAllowed(token) : _euRule.IsRequestAllowed(token);
    }
}
