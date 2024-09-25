namespace RateLimiter.Rules;

public class RestrictedRegionRuleset : BaseRuleset
{
    public RestrictedRegionRuleset()
    { 
        Add(new FalseRule("Danger!"));
        Add(new FalseRule("More Danger!!"));
    }
}