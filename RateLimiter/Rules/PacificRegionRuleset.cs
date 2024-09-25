namespace RateLimiter.Rules;

public class PacificRegionRuleset : BaseRuleset
{
    public PacificRegionRuleset()
    {
        Add(new BooleanRule("Pacific Boolean"));
        Add(new TrueRule("Pacific Region Boolean"));
    }
}