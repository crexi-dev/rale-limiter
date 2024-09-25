namespace RateLimiter.Rules;

public class UsRuleSet : BaseRuleset
{
    public UsRuleSet()
    {
        Add(new TrueRule("UsDefaultRuleset - true rule"));
    }
}