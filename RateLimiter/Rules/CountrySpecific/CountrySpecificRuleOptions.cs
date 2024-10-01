using System.Collections.Generic;

namespace RateLimiter.Rules.CountrySpecific
{
    public record CountrySpecificRuleOptions(IDictionary<string, IEnumerable<IRule>> Mapping)
    {
    }
}
