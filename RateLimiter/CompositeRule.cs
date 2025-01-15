using System.Collections.Generic;

namespace RateLimiter;

public class CompositeRule
{
    private readonly IEnumerable<Rule> _rules;

    public CompositeRule(IEnumerable<Rule> rules)
    {
        _rules = rules;
    }
}
