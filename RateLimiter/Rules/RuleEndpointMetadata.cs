using System;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class RuleEndpointMetadata
    {
        private HashSet<string> rules = new();

        public RuleEndpointMetadata(params string[] ruleNames)
        {
            if (ruleNames == null || ruleNames.Length == 0)
            {
                throw new ArgumentException(nameof(ruleNames));
            }

            foreach (var ruleName in ruleNames)
            {
                if (!this.rules.Contains(ruleName))
                {
                    this.rules.Add(ruleName);
                }
            }
        }

        public IEnumerable<string> Rules => this.rules;
    }
}
