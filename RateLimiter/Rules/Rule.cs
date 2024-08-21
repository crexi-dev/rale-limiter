using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class Rule
    {
        public IRuleAlg RuleAlg { get; set; }
        public List<string> LocationFilter { get; set; }

        public Rule(IRuleAlg ruleAlg, List<string> locationFilter = null)
        {
            RuleAlg = ruleAlg;
            LocationFilter = locationFilter;
        }

        public Rule() { }

        /// <summary>
        /// Mimics sending a request through the rate limiter rule.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>Whether or not request can go through.</returns>
        public bool SendRequest(string accessToken)
        {
            var loc = TokenUtilities.ExtractLocation(accessToken);
            if (LocationFilter != null && LocationFilter.Count > 0 && !LocationFilter.Contains(loc)) return true;

            var res = RuleAlg.SendRequest();
            return res;
        }

    }
}