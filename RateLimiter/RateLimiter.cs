using RateLimiter.Abstractions;
using RateLimiter.Config;

using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

public class RateLimiter : IRateLimitRequests
{
    private readonly IEnumerable<IRateLimitRule> _rules;

    public RateLimiter(
        IProvideRateLimitRules rulesFactory)
    {
        _rules = rulesFactory.GetRules();
    }

    public (bool, string) IsRequestAllowed(IEnumerable<RateLimitedResource> rateLimitedResources)
    {
        // need to get the discriminator for each incoming rale limit configuration
        var discriminators = GetDiscriminators(); //key: name value: discriminatorValue

        // get the matching rules
        var rules = _rules.Where(r => rateLimitedResources.Select(x => x.RuleName)
            .ToList().Contains(r.Name));

        // ensure they all pass
        var passed = rules.All(x => x.IsAllowed(x.Discriminator.ToString()));

        return passed ? (passed, string.Empty) :
            (passed, "some message about banging on our door too much");
    }

    private List<(string, string)> GetDiscriminators()
    {
        return [("foo", "bar)")];
    }
}