using RateLimiter.Examples.Rules;

namespace RateLimiter.Examples.Policies;

public class MyCustomSimplePolicy : IRateLimiterPolicy
{
    public IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request)
    {
        yield return new MyCustomBanEveryoneRule();
    }
}




