
namespace RateLimiter.Rules;

public class EuropeRuleset: BaseRuleset
{
    public EuropeRuleset()
    {
        Add(new BooleanRule("EuropeRuleset"));
    }
}