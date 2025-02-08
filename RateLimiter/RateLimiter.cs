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
        // get the matching rules
        var rules = _rules.Where(r => rateLimitedResources.Select(x => x.Discriminator)
            .ToList().Contains(r.Discriminator));

        // ensure they all pass
        var passed = rules.All(x => x.IsAllowed(x.Discriminator.ToString()));

        return passed ? (passed, string.Empty) :
            (passed, "some message about banging on our door too much");
    }
}